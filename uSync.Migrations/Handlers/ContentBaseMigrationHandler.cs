using System.Xml.Linq;

using Examine;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Polly;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;
using uSync.Core;
using uSync.Migrations.Composing;
using uSync.Migrations.Extensions;
using uSync.Migrations.Models;
using uSync.Migrations.Notifications;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;

internal class ContentBaseMigrationHandler<TEntity>
    where TEntity : IEntity
{
    private readonly IEventAggregator _eventAggregator;
    private readonly SyncPropertyMigratorCollection _migrators;
    private readonly SyncMigrationFileService _migrationFileService;
    private readonly IShortStringHelper _shortStringHelper;

    protected readonly HashSet<string> _ignoredProperties = new(StringComparer.OrdinalIgnoreCase);

    public ContentBaseMigrationHandler(
        IEventAggregator eventAggregator,
        SyncMigrationFileService migrationFileService,
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
        var editorAlias = context.GetEditorAlias(contentType, property.Name.LocalName);
        if (editorAlias.IsVortoEditorAlias() && itemType == "Content")
        {
            // vorto we split into many values 
            var vortoElement = GetVortoValues(property, context);
            if (vortoElement != null) return vortoElement;
        }

        // else - single value 
        var migratedValue = MigrateContentValue(editorAlias, property.Value, context);
        return new XElement(property.Name.LocalName, 
                    new XElement("Value", new XCData(migratedValue)));

    }
    
    /// <summary>
    ///  special case, spit a vorto value into multiple cultures, 
    ///  and return them back as a blob of xml values
    /// </summary>
    private XElement? GetVortoValues(XElement property, SyncMigrationContext context)
    {
        var attempt = property.Value.ConvertToVortoValue();
        if (attempt.Success && attempt.Result != null)
        {
            var newProperty = new XElement(property.Name.LocalName);

            // get editor alias from dtdguid
            var vortoEditorAlias = context.GetDataTypeFromDefinition(attempt.Result.DtdGuid);
            if (vortoEditorAlias != null)
            {
                foreach (var language in attempt.Result.Values)
                {
                    var migratedValue = MigrateContentValue(vortoEditorAlias, language.Value, context);

                    newProperty.Add(new XElement("Value",
                        new XAttribute("Culture", language.Key),
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

        foreach(var property in propertiesNode.Elements())
        {
            foreach(var value in property.Elements())
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

    private string MigrateContentValue(string editorAlias, string value, SyncMigrationContext context)
    {
        if (_migrators.TryGet(editorAlias, out var migrator) == true)
        {
            return migrator?.GetContentValue(editorAlias, value, context) ?? value;
        }

        return value;
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
