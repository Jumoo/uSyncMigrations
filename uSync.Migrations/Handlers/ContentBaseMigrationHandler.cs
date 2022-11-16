using System.Xml.Linq;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

using uSync.Core;
using uSync.Migrations.Composing;
using uSync.Migrations.Migrators;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Models;
using uSync.Migrations.Notifications;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;

internal class ContentBaseMigrationHandler<TEntity>
    where TEntity : IEntity
{
    public string Group => uSync.BackOffice.uSyncConstants.Groups.Content;

    private readonly IEventAggregator _eventAggregator;
    private readonly SyncPropertyMigratorCollection _migrators;
    private readonly ISyncMigrationFileService _migrationFileService;
    private readonly IShortStringHelper _shortStringHelper;

    protected readonly HashSet<string> _ignoredProperties = new(StringComparer.OrdinalIgnoreCase);

    public ContentBaseMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        SyncPropertyMigratorCollection contentPropertyMigrators,
        IShortStringHelper shortStringHelper)
    {
        _eventAggregator = eventAggregator;
        _migrationFileService = migrationFileService;
        _migrators = contentPropertyMigrators;
        _shortStringHelper = shortStringHelper;
    }

    public IEnumerable<MigrationMessage> DoMigrateFromDisk(Guid id, string folder, SyncMigrationContext context)
    {
        if (!Directory.Exists(folder)) return Enumerable.Empty<MigrationMessage>();

        // loads all the content names into the context, so we can get them later on.
        PrepareContext(folder, context);

        var itemType = Path.GetFileName(folder);

        return MigrateFolder(id, itemType, folder, 0, context);
    }

    private IEnumerable<MigrationMessage> MigrateFolder(Guid id, string itemType, string folder, int level, SyncMigrationContext context)
    {
        if (Directory.Exists(folder) == false)
        {
            return Enumerable.Empty<MigrationMessage>();
        }

        var messages = new List<MigrationMessage>();

        foreach (var file in Directory.GetFiles(folder, "*.config"))
        {
            var source = XElement.Load(file);

            var migratingNotification = new SyncMigratingNotification<TEntity>(source, context);

            if (_eventAggregator.PublishCancelable(migratingNotification) == true)
            {
                continue;
            }

            var target = ConvertContent(itemType, source, level, context);

            if (target != null)
            {
                var migratedNotification = new SyncMigratedNotification<TEntity>(target, context).WithStateFrom(migratingNotification);

                _eventAggregator.Publish(migratedNotification);

                messages.Add(SaveTargetXml(itemType, id, target));
            }
        }

        foreach (var childFolder in Directory.GetDirectories(folder))
        {
            messages.AddRange(MigrateFolder(id, itemType, childFolder, level + 1, context));
        }

        return messages;
    }

    private XElement ConvertContent(string itemType, XElement source, int level, SyncMigrationContext context)
    {
        var key = source.Attribute("guid").ValueOrDefault(Guid.Empty);
        var alias = source.Attribute("nodeName").ValueOrDefault(string.Empty);
        var parent = source.Attribute("parentGUID").ValueOrDefault(Guid.Empty);
        var contentType = source.Attribute("nodeTypeAlias").ValueOrDefault(string.Empty);
        var template = source.Attribute("templateAlias").ValueOrDefault(string.Empty);
        var published = source.Attribute("published").ValueOrDefault(false);
        var createdDate = source.Attribute("updated").ValueOrDefault(DateTime.Now);
        var sortOrder = source.Attribute("sortOrder").ValueOrDefault(0);

        var path = context.GetContentPath(parent) + "/" + alias.ToSafeAlias(_shortStringHelper);

        // content is blocked by path (e.g home/about-us)

        if (context.IsBlocked(itemType, path)) return null;

        context.AddContentPath(key, path);

        var target = new XElement(itemType,

            new XAttribute("Key", key),
            new XAttribute("Alias", alias),
            new XAttribute("Level", level),

            new XElement("Info",
                new XElement("Parent", new XAttribute("Key", parent), context.GetContentAlias(parent)),
                new XElement("Path", path),
                new XElement("Trashed", false),
                new XElement("ContentType", contentType),
                new XElement("CreateDate", createdDate.ToString("s")),
                new XElement("NodeName", new XAttribute("Default", alias)),
                new XElement("SortOrder", sortOrder)));

        if (itemType == "Content")
        {
            var info = target.Element("Info");

            info.Add(new XElement("Published", new XAttribute("Default", published)));
            info.Add(new XElement("Schedule"));

            if (string.IsNullOrWhiteSpace(template) == false)
            {
                info.Add(new XElement("Template", new XAttribute("Key", context.GetTemplateKey(template)), template));
            }
            else
            {
                info.Add(new XElement("Template"));
            }
        }

        var propertiesList = new XElement("Properties");

        foreach (var property in source.Elements())
        {
            if (_ignoredProperties.Contains(property.Name.LocalName))
            {
                continue;
            }

            if (context.IsIgnoredProperty(contentType, property.Name.LocalName))
            {
                continue;
            }

            var newProperty = ConvertPropertyValue(itemType, contentType, property, context);
            if (newProperty != null)
            {
                propertiesList.Add(newProperty);
            }
        }

        target.Add(propertiesList);

        // check we have language title / and published statuses
        EnsureLanguageTitles(target);

        return target;
    }

    private XElement ConvertPropertyValue(string itemType, string contentType, XElement property, SyncMigrationContext context)
    {
        var editorAlias = context.GetEditorAlias(contentType, property.Name.LocalName)?.OrginalEditorAlias ?? string.Empty;

        // convert the property .

        var migrationProperty = new SyncMigrationContentProperty(editorAlias, property.Value);
        var migrator = _migrators.GetVariantMigrator(editorAlias);
        if (migrator != null && itemType == "Content")
        {
            // it might be the case that the property needs to be split into variants. 
            // if this is the case a ISyncVariationPropertyEditor will exist and it can 
            // split a single value into a collection split by culture
            var vortoElement = GetVariedValueNode(migrator, property.Name.LocalName, migrationProperty, context);
            if (vortoElement != null) return vortoElement;
        }

        // or this value doesn't need to be split by variation
        // and we can 'just' migrate it on its own.
        var migratedValue = MigrateContentValue(migrationProperty, context);
        return new XElement(property.Name.LocalName,
                    new XElement("Value", new XCData(migratedValue)));
    }

    /// <summary>
    ///  special case, spit a vorto value into multiple cultures, 
    ///  and return them back as a blob of xml values
    /// </summary>
    private XElement? GetVariedValueNode(ISyncVariationPropertyMigrator migrator, string propertyName, SyncMigrationContentProperty migrationProperty, SyncMigrationContext context)
    {
        // Get varied elements from the migrator.
        var attempt = migrator.GetVariedElements(migrationProperty, context);
        if (attempt.Success && attempt.Result != null)
        {
            // this returns an object which tells us what datatype to use
            // and a dictionary of cultuire / values we can migrate. 

            var newProperty = new XElement(propertyName);

            // get editor alias from dtdguid
            var variantEditorAlias = context.GetDataTypeFromDefinition(attempt.Result.DtdGuid);
            if (variantEditorAlias != null)
            {
                foreach (var variation in attempt.Result.Values)
                {
                    var variationProperty = new SyncMigrationContentProperty(variantEditorAlias, variation.Value);

                    var migratedValue = MigrateContentValue(variationProperty, context);

                    newProperty.Add(new XElement("Value",
                        new XAttribute("Culture", variation.Key),
                        new XCData(migratedValue)));
                }
            }

            return newProperty;
        }

        return null;
    }

    /// <summary>
    ///  at the end - if we have added vorto values, we also need
    ///  to add nodename titles for the languages. 
    /// </summary>
    /// <param name="node"></param>
    private void EnsureLanguageTitles(XElement node)
    {
        var propertiesNode = node.Element("Properties");
        if (propertiesNode == null) return;

        var nodeNameNode = node.Element("Info")?.Element("NodeName");
        if (nodeNameNode == null) return;

        var languages = new List<string>();

        foreach (var property in propertiesNode.Elements())
        {
            foreach (var value in property.Elements())
            {
                var culture = value.Attribute("Culture").ValueOrDefault(string.Empty).ToLower();
                if (!string.IsNullOrWhiteSpace(culture)
                    && !languages.Contains(culture))
                {
                    languages.Add(culture);
                }
            }
        }

        if (languages.Count > 0)
        {
            var defaultName = nodeNameNode.Attribute("Default").ValueOrDefault(string.Empty);
            var publishedNode = node.Element("Info")?.Element("Published");
            var publishedValue = publishedNode?.Attribute("Default").ValueOrDefault(false) ?? false;

            foreach (var language in languages)
            {
                nodeNameNode.Add(new XElement("Name",
                    new XAttribute("Culture", language), defaultName));

                if (publishedNode != null)
                {
                    publishedNode.Add(new XElement("Published",
                        new XAttribute("Culture", language), publishedValue));
                }
            }
        }
    }

    private MigrationMessage SaveTargetXml(string itemType, Guid id, XElement xml)
    {
        _migrationFileService.SaveMigrationFile(id, xml, xml.Name.LocalName);

        return new MigrationMessage(itemType, xml.GetAlias(), MigrationMessageType.Success);
    }

    private string MigrateContentValue(SyncMigrationContentProperty migrationProperty, SyncMigrationContext context)
    {
        if (string.IsNullOrWhiteSpace(migrationProperty?.EditorAlias)) return migrationProperty.Value;

        if (_migrators.TryGet(migrationProperty?.EditorAlias, out var migrator) == true)
        {
            return migrator?.GetContentValue(migrationProperty, context) ?? migrationProperty.Value;
        }

        return migrationProperty.Value;
    }

    private void PrepareContext(string folder, SyncMigrationContext context)
    {
        var files = Directory.GetFiles(folder, "*.config", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var source = XElement.Load(file);
            var key = source.Attribute("guid").ValueOrDefault(Guid.Empty);
            var alias = source.Attribute("nodeName").ValueOrDefault(string.Empty);

            if (key != Guid.Empty && string.IsNullOrWhiteSpace(alias) == false)
            {
                context.AddContentKey(key, alias);
            }
        }
    }
}
