using System.Xml.Linq;

using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using uSync.Core;
using uSync.Migrations.Context;
using uSync.Migrations.Handlers.Shared;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Seven;

internal abstract class ContentTypeBaseMigrationHandler<TEntity> : SharedContentTypeBaseHandler<TEntity>
    where TEntity : ContentTypeBase
{
    public ContentTypeBaseMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        ILogger<ContentTypeBaseMigrationHandler<TEntity>> logger,
        IDataTypeService dataTypeService)
        : base(eventAggregator, migrationFileService, logger, dataTypeService)
    { }

    protected override (string alias, Guid key) GetAliasAndKey(XElement source)
        => (
            alias: source.Element("Info")?.Element("Alias")?.ValueOrDefault(string.Empty) ?? string.Empty,
            key: source.Element("Info")?.Element("Key")?.ValueOrDefault(Guid.Empty) ?? Guid.Empty
        );

    protected override void UpdateTabs(XElement source, XElement target, SyncMigrationContext context)
    {
        var tabs = source.Element("Tabs");
        if (tabs != null)
        {
            var newTabs = new XElement("Tabs");
            foreach (var tab in tabs.Elements("Tab"))
            {
                var newTab = XElement.Parse(tab.ToString());
                newTab = UpdateTab(source, newTab, context);
                if (newTab != null) newTabs.Add(newTab);
            }
            target.Add(newTabs);
        }
    }

    protected override void UpdatePropertyXml(XElement source, XElement newProperty, SyncMigrationContext context)
    {
        newProperty.Add(new XElement("MandatoryMessage", string.Empty));
        newProperty.Add(new XElement("ValidationRegExpMessage", string.Empty));
        newProperty.Add(new XElement("LabelOnTop", false));

        var tabNode = newProperty.Element("Tab");
        UpdateTab(source, tabNode, context);
    }

    internal XElement? UpdateTab(XElement source, XElement tab, SyncMigrationContext context)
    {
        var renamedTabs = context.GetChangedTabs();

        var caption = tab.Element("Caption").ValueOrDefault(tab.ValueOrDefault(string.Empty));
        var alias = caption.Replace(" ", "").ToFirstLower();
        var deleteTab = false;

        if (renamedTabs.Select(x => x.OriginalName).Contains(caption))
        {
            var tabMatch = renamedTabs.Where(x => x.OriginalName == caption).FirstOrDefault();
            if (tabMatch != null)
            {
                if (string.IsNullOrWhiteSpace(tabMatch.NewName)) deleteTab = true;
                alias = !string.IsNullOrWhiteSpace(tabMatch.Alias) ? tabMatch.Alias : tabMatch.NewName;
                caption = tabMatch.NewName;
            }
        }

        if (!deleteTab)
        {
            if (tab.Element("Key") == null)
            {
                var (sourceAlias, sourceKey) = GetAliasAndKey(source);
                var newAlias = sourceAlias + alias;
                tab.Add(new XElement("Key", newAlias.ToGuid().ToString()));
            }
            if (tab.Element("Caption") != null)
            {
                tab.Element("Caption").Value = caption;
            }
            else
            {
                tab.Value = caption;
            }
            tab.SetAttributeValue("Alias", alias);
            tab.SetAttributeValue("Type", "Tab");
            return tab;
        }
        return null;
    }

    /// <summary>
    ///  update the info section, with the new things that are in v8+ that have no equivalent in v7
    /// </summary>
    protected override void UpdateInfoSection(XElement? info, XElement target, Guid key, SyncMigrationContext context)
    {
        if (info == null) return;

        var targetInfo = XElement.Parse(info.ToString());
        targetInfo.Element("Key")?.Remove();
        targetInfo.Element("Alias")?.Remove();

        targetInfo.Add(new XElement("Variations", "Nothing"));
        targetInfo.Add(new XElement("IsElement", context.ContentTypes.IsElementType(key)));

        target.Add(targetInfo);
    }

    /// <summary>
    ///  update the structure (allowed nodes)
    /// </summary>
    protected override void UpdateStructure(XElement source, XElement target)
    {
        var sourceStructure = source.Element("Structure");

        if (sourceStructure != null)
        {
            var i = 0;
            var transformedStructure = new XElement("Structure");
            foreach (var element in sourceStructure.Elements())
            {
                var contentType = new XElement("ContentType");
                contentType.SetAttributeValue("Key", element?.Attribute("Key")?.Value);
                contentType.SetAttributeValue("SortOrder", i);
                contentType.Value = element.Value;

                transformedStructure.Add(contentType);
                i++;
            }
            target.Add(XElement.Parse(transformedStructure.ToString()));
        }
    }

    protected override void CheckVariations(XElement target)
    {
        if (target.Element("Info") == null) return;

        var contentTypeVariations = "Nothing";

        var properties = target.Element("GenericProperties");
        if (properties != null)
        {
            foreach (var property in properties.Elements("GenericProperty"))
            {
                var variations = property.Element("Variations").ValueOrDefault(string.Empty);
                if (variations != "Nothing")
                {
                    contentTypeVariations = variations;
                    break;
                }
            }
        }

        target.Element("Info").CreateOrSetElement("Variations", contentTypeVariations);
    }
}
