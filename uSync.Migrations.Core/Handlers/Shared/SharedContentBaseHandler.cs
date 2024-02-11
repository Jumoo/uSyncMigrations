﻿using System.Xml.Linq;

using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

using uSync.Core;
using uSync.Migrations.Core.Context;
using uSync.Migrations.Core.Extensions;
using uSync.Migrations.Core.Migrators;
using uSync.Migrations.Core.Migrators.Models;
using uSync.Migrations.Core.Models;
using uSync.Migrations.Core.Services;

namespace uSync.Migrations.Core.Handlers.Shared;
public abstract class SharedContentBaseHandler<TEntity> : SharedHandlerBase<TEntity>
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
        var (alias, key) = GetAliasAndKey(source, context);

        if (key != Guid.Empty)
        {
            var entityType = this.GetEntityType();
            if (entityType != null)
            {
                context.AddEntityMap(key, entityType);
            }

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

    protected abstract string? GetEntityType();

    protected abstract XElement GetBaseXml(XElement source, Guid parent, string contentType, int level, SyncMigrationContext context);

    protected abstract Guid GetParent(XElement source);

    protected abstract string GetContentType(XElement source, SyncMigrationContext context);

    protected abstract string GetPath(string alias, Guid parent, SyncMigrationContext context);

    protected abstract IEnumerable<XElement>? GetProperties(XElement source);

    protected abstract string GetNewContentType(string contentType, XElement source);

    protected override XElement? MigrateFile(XElement source, int level, SyncMigrationContext context)
    {
        var (alias, key) = GetAliasAndKey(source, context);

        var parent = GetParent(source);
        var contentType = GetContentType(source, context);

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


        if (context.Content.TryGetMergedProperties(contentType, out var mergedProperties) is true)
        {
            MergeContentProperties(contentType, mergedProperties, propertiesList, context);
        }


        target.Add(propertiesList);

        // check we have language title / and published statuses
        EnsureLanguageTitles(target);

        return target;
    }

    protected virtual IEnumerable<XElement> ConvertPropertyValue(string itemType, string contentType, XElement property, SyncMigrationContext context)
    {
        string? editorAlias = context.ContentTypes.TryGetEditorAliasByTypeAndProperty(contentType, property.Name.LocalName, out var editorInfo) is false
            ? string.Empty
            : editorInfo.OriginalEditorAlias ?? string.Empty;

        try
        {

            // convert the property .
            _logger.LogDebug("ConvertPropertyValue: {itemType} {contentType} {editorAlias}", itemType, contentType, editorAlias);

            var migrationProperty = new SyncMigrationContentProperty(
                contentType, property.Name.LocalName, editorAlias, property.Value);

            if (context.Migrators.TryGetMigrator($"{migrationProperty.ContentTypeAlias}_{migrationProperty.PropertyAlias}", migrationProperty.EditorAlias, out var propertyMigrator) is true)
            {
                switch (propertyMigrator)
                {
                    case ISyncVariationPropertyMigrator variationPropertyMigrator:
                        _logger.LogDebug("Variation Migrator {name}", variationPropertyMigrator.GetType().Name);
                        var variationResult = GetVariedValueNode(variationPropertyMigrator, contentType, property.Name.LocalName, migrationProperty, context);
                        if (variationResult != null) return variationResult.AsEnumerableOfOne();
                        break;
                    case ISyncPropertySplittingMigrator splittingMigrator:
                        _logger.LogDebug("Splitting migrator {name}", splittingMigrator.GetType().Name);
                        return GetSplitPropertyValueNodes(splittingMigrator, contentType, property.Name.LocalName, migrationProperty, context);
                }
            }

            // default this value doesn't need to be split
            // and we can 'just' migrate it on its own.
            var migratedValue = MigrateContentValue(migrationProperty, context);
            return new XElement(property.Name.LocalName,
                        new XElement("Value", new XCData(migratedValue))).AsEnumerableOfOne();
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to migrate property [{editorAlias} {property}] {ex}",
                editorAlias, property.Name.LocalName, ex.Message);
            throw new Exception($"Failed migrating [{editorAlias} - {property.Name.LocalName}] : {ex.Message}", ex);
        }
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
        var newProperty = new XElement(propertyName);

        // Get varied elements from the migrator.
        var attempt = migrator.GetVariedElements(migrationProperty, context);
        if (attempt.Success && attempt.Result?.Values != null)
        {
            // this returns an object which tells us what datatype to use
            // and a dictionary of culture / values we can migrate.

            // get editor alias from dtdguid
            if (context.DataTypes.TryGetInfoByDefinition(attempt.Result.DtdGuid, out var variantDataType) is false) { return newProperty; }

            foreach (var variation in attempt.Result.Values)
            {
                var variationProperty = new SyncMigrationContentProperty(
                    contentTypeAlias, propertyName,
                    variantDataType.EditorAlias, variation.Value);

                var migratedValue = MigrateContentValue(variationProperty, context);

                newProperty.Add(new XElement("Value",
                    new XAttribute("Culture", variation.Key),
                    new XCData(migratedValue)));
            }
        }

        return newProperty;
    }

    protected virtual string MigrateContentValue(SyncMigrationContentProperty migrationProperty, SyncMigrationContext context)
    {
        if (migrationProperty?.Value == null)
        {
            _logger.LogDebug("{info} value is null, nothing to migrate",
                migrationProperty?.GetDetailString() ?? "(Blank)");

            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(migrationProperty.EditorAlias))
        {
            _logger.LogDebug("{info} has a blank editorAlias, can't migrate",
                migrationProperty.GetDetailString());

            return migrationProperty.Value;
        }

        if (context.Migrators.TryGetMigrator($"{migrationProperty.ContentTypeAlias}_{migrationProperty.PropertyAlias}", migrationProperty.EditorAlias, out var migrator) is true)
        {
            _logger.LogDebug("{info} migrating with {migrator}",
                migrationProperty.GetDetailString(), migrator.GetType().Name);

            return migrator.GetContentValue(migrationProperty, context) ?? migrationProperty.Value;
        }

        _logger.LogDebug("{info}, no migrator, value will not change",
            migrationProperty.GetDetailString());

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

    /// <summary>
    ///  Merge multiple properties into one single property. 
    /// </summary>
    /// <remarks>
    ///     this method gathers the required properties from all the properties, 
    ///     and then by culture will pass them on to a MergeProperties 'migrator' or something 
    ///     to do the actual work.
    /// </remarks>
    protected virtual void MergeContentProperties(string contentType, MergingPropertiesConfig config, XElement? propertiesElement, SyncMigrationContext context)
    {
        // don't do the merge if... 
        // we can merge all this, but this is readable. 
        if (propertiesElement == null) return;

        if (context.Migrators.TryGetMergingMigrator(contentType, out var merger) is false) { return; }

        if (config == null) return;
        if (!config.MergedProperties.Any()) return;

        var mergingPropertiesByCulture = GetMergingPropertiesByCulture(contentType, config, propertiesElement, context);
        if (mergingPropertiesByCulture.Count == 0) return; // nothing to merge 

        var targetElement = MergePropertiesToXElement(mergingPropertiesByCulture, merger, config, context);
        if (targetElement != null)
            propertiesElement.Add(targetElement);

        if (config.RemoveMergedProperties)
            propertiesElement.RemoveByName(config.MergedProperties);
    }

    /// <summary>
    ///  get the properties and values that we want to merge sorted by culture. 
    /// </summary>
    private Dictionary<string, List<MergingPropertyValue>> GetMergingPropertiesByCulture(string contentType, MergingPropertiesConfig config, XElement propertyElementValues, SyncMigrationContext context)
    {
        Dictionary<string, List<MergingPropertyValue>> mergingPropertiesByCulture = new();

        foreach (var property in propertyElementValues.Elements())
        {
            if (!config.MergedProperties.InvariantContains(property.Name.LocalName)) continue;

            string? editorAlias = context.ContentTypes.TryGetEditorAliasByTypeAndProperty(contentType, property.Name.LocalName, out var editorInfo) is false
                ? string.Empty
                : editorInfo.OriginalEditorAlias ?? string.Empty;


            var propertyValuesByCulture = property.GetPropertyValueByCultures();
            if (propertyValuesByCulture == null) continue;

            foreach (var cultureValue in propertyValuesByCulture)
            {
                if (!mergingPropertiesByCulture.ContainsKey(cultureValue.Key))
                    mergingPropertiesByCulture[cultureValue.Key] = new();

                mergingPropertiesByCulture[cultureValue.Key].Add(
                    new MergingPropertyValue(property.Name.LocalName, editorAlias, cultureValue.Value));
            }
        }

        return mergingPropertiesByCulture;
    }

    /// <summary>
    ///  get the merged value and create hte value element required (by culture if needed)
    /// </summary>
    private XElement? MergePropertiesToXElement(Dictionary<string, List<MergingPropertyValue>> mergingPropertiesByCulture, ISyncPropertyMergingMigrator merger, MergingPropertiesConfig config, SyncMigrationContext context)
    {
        if (merger == null) return null;

        var targetElement = new XElement(config.TargetPropertyAlias);
        // merge the properties. 
        foreach (var cultureProperties in mergingPropertiesByCulture)
        {
            var mergedValue = merger.GetMergedContentValues(cultureProperties.Value, context);
            if (string.IsNullOrEmpty(mergedValue)) continue;

            // add this as a value element to the xml
            targetElement.Add(mergedValue.CreateValueElement(cultureProperties.Key));
        }

        // add the new target to the element.
        return targetElement;
    }
}
