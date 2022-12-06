using System.Xml.Linq;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

using uSync.Core;
using uSync.Migrations.Migrators;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;

internal abstract class ContentBaseMigrationHandler<TEntity> : MigrationHandlerBase<TEntity>
    where TEntity : IEntity
{
    private readonly IShortStringHelper _shortStringHelper;

    protected readonly HashSet<string> _ignoredProperties = new(StringComparer.OrdinalIgnoreCase);
    protected readonly Dictionary<string, string> _mediaTypeAliasForFileExtension = new(StringComparer.OrdinalIgnoreCase);

    public ContentBaseMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        IShortStringHelper shortStringHelper)
        : base(eventAggregator, migrationFileService)
    {
        _shortStringHelper = shortStringHelper;
    }

    protected override void PrepareFile(XElement source, SyncMigrationContext context)
    {
        var key = source.Attribute("guid").ValueOrDefault(Guid.Empty);
        if (key != Guid.Empty)
        {
            var id = source.Attribute("id").ValueOrDefault(0);
            var alias = source.Attribute("nodeName").ValueOrDefault(string.Empty);

            if (id > 0)
            {
                context.AddKey(id, key);
            }

            if (string.IsNullOrWhiteSpace(alias) == false)
            {
                context.AddContentKey(key, alias);
            }
        }
    }

    protected override XElement? MigrateFile(XElement source, int level, SyncMigrationContext context)
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

        if (context.IsBlocked(ItemType, path)) return null;

        context.AddContentPath(key, path);

        if (ItemType == nameof(Media) && _mediaTypeAliasForFileExtension.Count > 0)
        {
            var fileExtension = source.Element(UmbConstants.Conventions.Media.Extension)?.ValueOrDefault(string.Empty) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fileExtension) == false && _mediaTypeAliasForFileExtension.TryGetValue(fileExtension, out var newMediaTypeAlias) == true)
            {
                contentType = newMediaTypeAlias;
            }
        }

        var target = new XElement(ItemType,

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

        if (ItemType == nameof(Content))
        {
            var info = target.Element("Info");
            if (info != null)
            {
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

            var newProperty = ContentBaseMigrationHandler<TEntity>.ConvertPropertyValue(ItemType, contentType, property, context);
            if (newProperty != null)
            {
                propertiesList.Add(newProperty);
            }
        }

        target.Add(propertiesList);

        // check we have language title / and published statuses
        ContentBaseMigrationHandler<TEntity>.EnsureLanguageTitles(target);

        return target;
    }

    private static XElement ConvertPropertyValue(string itemType, string contentType, XElement property, SyncMigrationContext context)
    {
        var editorAlias = context.GetEditorAlias(contentType, property.Name.LocalName)?.OriginalEditorAlias ?? string.Empty;

        // convert the property .

        var migrationProperty = new SyncMigrationContentProperty(editorAlias, property.Value);
        var migrator = context.TryGetVariantMigrator(editorAlias);
        if (migrator != null && itemType == "Content")
        {
            // it might be the case that the property needs to be split into variants. 
            // if this is the case a ISyncVariationPropertyEditor will exist and it can 
            // split a single value into a collection split by culture
            var vortoElement = ContentBaseMigrationHandler<TEntity>.GetVariedValueNode(migrator, property.Name.LocalName, migrationProperty, context);
            if (vortoElement != null) return vortoElement;
        }

        // or this value doesn't need to be split by variation
        // and we can 'just' migrate it on its own.
        var migratedValue = ContentBaseMigrationHandler<TEntity>.MigrateContentValue(migrationProperty, context);
        return new XElement(property.Name.LocalName,
                    new XElement("Value", new XCData(migratedValue)));
    }

    /// <summary>
    ///  special case, spit a vorto value into multiple cultures, 
    ///  and return them back as a blob of xml values
    /// </summary>
    private static XElement? GetVariedValueNode(ISyncVariationPropertyMigrator migrator, string propertyName, SyncMigrationContentProperty migrationProperty, SyncMigrationContext context)
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

                    var migratedValue = ContentBaseMigrationHandler<TEntity>.MigrateContentValue(variationProperty, context);

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
    private static void EnsureLanguageTitles(XElement node)
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

                publishedNode?.Add(new XElement("Published",
                        new XAttribute("Culture", language), publishedValue));
            }
        }
    }

    private static string MigrateContentValue(SyncMigrationContentProperty migrationProperty, SyncMigrationContext context)
    {
        if (migrationProperty == null) return string.Empty;

        if (string.IsNullOrWhiteSpace(migrationProperty.EditorAlias)) return migrationProperty.Value;

        var migrator = context.TryGetMigrator(migrationProperty.EditorAlias);
        if (migrator != null)
        {
            return migrator?.GetContentValue(migrationProperty, context) ?? migrationProperty.Value;
        }

        return migrationProperty.Value;
    }
}
