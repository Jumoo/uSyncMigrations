using System.Xml.Linq;

using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;
using uSync.Core;
using uSync.Migrations.Context;
using uSync.Migrations.Migrators;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Shared;
internal abstract class SharedContentBaseHandler<TEntity> : SharedHandlerBase<TEntity>
    where TEntity : ContentBase
{
    protected readonly IShortStringHelper _shortStringHelper;
    
    protected readonly HashSet<string> _ignoredProperties = new(StringComparer.OrdinalIgnoreCase);
    protected readonly Dictionary<string, string> _mediaTypeAliasForFileExtension = new(StringComparer.OrdinalIgnoreCase);

    public SharedContentBaseHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        IShortStringHelper shortStringHelper,
        ILogger<SharedContentBaseHandler<TEntity>> logger) 
        : base(eventAggregator, migrationFileService, logger)
    {
        _shortStringHelper = shortStringHelper;
    }

    protected abstract int GetId(XElement source);

    protected override void PrepareFile(XElement source, SyncMigrationContext context)
    {
        var (alias, key) = GetAliasAndKey(source);

        if (key != Guid.Empty)
        {
            var id = GetId(source);
            if (id > 0)
            {
                context.AddKey(id, key);
            }

            if (string.IsNullOrWhiteSpace(alias) == false)
            {
                context.Content.AddKey(key, alias);
            }
        }
    }

    protected abstract XElement GetBaseXml(XElement source, Guid parent, string contentType, int level, SyncMigrationContext context);

    protected abstract Guid GetParent(XElement source);

    protected abstract string GetContentType(XElement source);

    protected abstract string GetPath(string alias, Guid parent, SyncMigrationContext context);

    protected abstract IEnumerable<XElement>? GetProperties(XElement source);

    protected abstract string GetNewContentType(string contentType, XElement source);

    protected override XElement? MigrateFile(XElement source, int level, SyncMigrationContext context)
    {
        var (alias, key) = GetAliasAndKey(source);

        var parent = GetParent(source);
        var contentType = GetContentType(source);

        var path = GetPath(alias, parent, context);
        context.Content.AddContentPath(key, path);

        // media might update this 
        contentType = GetNewContentType(contentType, source);

        var target = GetBaseXml(source, parent, contentType, level, context);

        var propertiesList = new XElement("Properties");

        var properties = GetProperties(source) ?? Enumerable.Empty<XElement>();
        foreach (var property in properties)
        {
            if (_ignoredProperties.Contains(property.Name.LocalName))
            {
                continue;
            }

            if (context.ContentTypes.IsIgnoredProperty(contentType, property.Name.LocalName))
            {
                continue;
            }

            var newProperty = ConvertPropertyValue(ItemType, contentType, property, context);
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

    protected virtual IEnumerable<XElement> ConvertPropertyValue(string itemType, string contentType, XElement property, SyncMigrationContext context)
    {
        var editorAlias = context.ContentTypes.GetEditorAliasByTypeAndProperty(contentType, property.Name.LocalName)?.OriginalEditorAlias ?? string.Empty;

        // convert the property .

        var migrationProperty = new SyncMigrationContentProperty(
            contentType, property.Name.LocalName, editorAlias, property.Value);

        var migrator = context.Migrators.TryGetVariantMigrator(editorAlias);
        if (migrator != null && itemType == "Content")
        {
            // it might be the case that the property needs to be split into variants. 
            // if this is the case a ISyncVariationPropertyEditor will exist and it can 
            // split a single value into a collection split by culture
            var vortoElement = GetVariedValueNode(migrator, contentType, property.Name.LocalName, migrationProperty, context);
            if (vortoElement != null) return vortoElement.AsEnumerableOfOne();
        }

        // we might want to split this property into multiple properties
        var propertySplittingMigrator = context.Migrators.TryGetPropertySplittingMigrator(editorAlias);
        if (propertySplittingMigrator != null)
        {
            var splitElements = GetSplitPropertyValueNodes(propertySplittingMigrator, contentType, property.Name.LocalName, migrationProperty, context);
            return splitElements;
        }
        
        // or this value doesn't need to be split
        // and we can 'just' migrate it on its own.
        var migratedValue = MigrateContentValue(migrationProperty, context);
        return new XElement(property.Name.LocalName,
                    new XElement("Value", new XCData(migratedValue))).AsEnumerableOfOne();
    }

    protected virtual IEnumerable<XElement> GetSplitPropertyValueNodes(ISyncPropertySplittingMigrator propertySplittingMigrator, string contentType, string propertyAlias, SyncMigrationContentProperty migrationProperty, SyncMigrationContext context)
    {
        var values = propertySplittingMigrator.GetContentValues(migrationProperty, context);
        foreach (var value in values)
        {
            var element = new XElement(value.Alias, new XElement("Value", value.Value));

            yield return element;
        }
    }

    /// <summary>
    ///  special case, spit a vorto value into multiple cultures, 
    ///  and return them back as a blob of xml values
    /// </summary>
    protected virtual XElement? GetVariedValueNode(ISyncVariationPropertyMigrator migrator, string contentTypeAlias, string propertyName, SyncMigrationContentProperty migrationProperty, SyncMigrationContext context)
    {
        // Get varied elements from the migrator.
        var attempt = migrator.GetVariedElements(migrationProperty, context);
        if (attempt.Success && attempt.Result != null)
        {
            // this returns an object which tells us what datatype to use
            // and a dictionary of cultuire / values we can migrate. 

            var newProperty = new XElement(propertyName);

            // get editor alias from dtdguid
            var variantEditorAlias = context.DataTypes.GetByDefinition(attempt.Result.DtdGuid);
            if (variantEditorAlias != null)
            {
                foreach (var variation in attempt.Result.Values)
                {
                    var variationProperty = new SyncMigrationContentProperty(
                        contentTypeAlias, propertyName,
                        variantEditorAlias, variation.Value);

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

    protected virtual string MigrateContentValue(SyncMigrationContentProperty migrationProperty, SyncMigrationContext context)
    {
        if (migrationProperty == null) return string.Empty;

        if (string.IsNullOrWhiteSpace(migrationProperty.EditorAlias)) return migrationProperty.Value;

        var migrator = context.Migrators.TryGetMigrator(migrationProperty.EditorAlias);
        if (migrator != null)
        {
            return migrator?.GetContentValue(migrationProperty, context) ?? migrationProperty.Value;
        }

        return migrationProperty.Value;
    }

    /// <summary>
    ///  something we probibly only need to do for v7 - but ensure we have 
    ///  all the right cultures in the title (nodename) as we do in the properties.
    /// </summary>
    /// <remarks>
    ///  v7 needs this when vorto splits something up, in v8 we don't so in theory
    ///  this should be fine, but this could act as a 'fix' in a v8 migration if 
    ///  something was missing.
    /// </remarks>

    protected virtual void EnsureLanguageTitles(XElement node)
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

}
