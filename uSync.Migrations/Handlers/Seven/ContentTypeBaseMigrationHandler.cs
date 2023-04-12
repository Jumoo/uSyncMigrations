using System.Xml.Linq;

using Microsoft.Extensions.Logging;

using NPoco;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

using uSync.Core;
using uSync.Migrations.Composing;
using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Handlers.Shared;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Seven;

internal abstract class ContentTypeBaseMigrationHandler<TEntity> : SharedContentTypeBaseHandler<TEntity>
    where TEntity : ContentTypeBase
{

    private readonly IShortStringHelper _shortStringHelper;

    public ContentTypeBaseMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        ILogger<ContentTypeBaseMigrationHandler<TEntity>> logger,
        IDataTypeService dataTypeService,
        IShortStringHelper shortStringHelper,
        Lazy<SyncMigrationHandlerCollection> migrationHandlers)
        : base(eventAggregator, migrationFileService, logger, dataTypeService, migrationHandlers)
    {
        _shortStringHelper = shortStringHelper;
    }

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
                var newTab = UpdateTab(source, tab.Clone(), context, false);
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
        UpdateTab(source, tabNode, context, true);
    }

    /// <summary>
    ///  Updates a tab to the new format.
    /// </summary>
    /// <remarks>   
    ///  Tabs in v8 have a key, alias and a type that doesn't exist in v7
    ///  
    ///  we also can rename and remove tabs by setting adding a ChangedTab to the context. 
    /// </remarks>
    internal XElement? UpdateTab(XElement source, XElement? tab, SyncMigrationContext context, bool useAttributes)
    {
        if (tab == null) return null;

        // get what the caption and alias will be for this tab. 
        var (caption, alias) = GetTabCaptionAndAlias(tab, context);
        if (string.IsNullOrWhiteSpace(caption) || string.IsNullOrWhiteSpace(alias)) return null;

        // add key if its missing 
        if (tab.Element("Key") == null)
        {
            var (sourceAlias, _) = GetAliasAndKey(source);
            tab.Add(new XElement("Key", $"{sourceAlias}{alias}".ToGuid()));
        }

        // add caption if the value is there 
        if (tab.Element("Caption") != null)
        {
            tab.Element("Caption")!.Value = caption;
        }
        else
        {
            tab.Value = caption;
        }

        // set alias, and type (always tab?)
        // add caption if the value is there 
        if (useAttributes)
        {
            tab.SetAttributeValue("Alias", alias);
            tab.SetAttributeValue("Type", "Tab");
        }
        else
        {
            tab.Elements("Alias").Remove();
            tab.Add(new XElement("Alias", alias));
            tab.Elements("Type").Remove();
            tab.Add(new XElement("Type", "Tab"));
        }

        return tab;
    }

    /// <summary>
    ///  works out what the tab name and alias should be. 
    /// </summary>
    /// <remarks>
    ///  if the context contains something saying we want to rename the tabs then we 
    ///  use that. 
    ///  
    ///  if a tab has been added to the rename, with a blank NewName - that is a delete
    ///  as we won't set any tabs blank captions/alias values. 
    /// </remarks>
    private (string? caption, string? alias) GetTabCaptionAndAlias(XElement tab, SyncMigrationContext context)
    {
        var renamedTabs = context.ContentTypes.GetChangedTabs();

        var caption = tab.Element("Caption").ValueOrDefault(tab.ValueOrDefault(string.Empty));
        var alias = caption.ToSafeAlias(_shortStringHelper);

        if (renamedTabs.Select(x => x.OriginalName).Contains(caption))
        {
            var tabMatch = renamedTabs.Where(x => x.OriginalName == caption).FirstOrDefault();
            if (tabMatch != null)
            {
                // if the new tabName is null, we are effecitfly deleting this by retuening null.
                if (tabMatch.DeleteTab || string.IsNullOrWhiteSpace(tabMatch.NewName)) return (null, null);

                alias = !string.IsNullOrWhiteSpace(tabMatch.Alias) ? tabMatch.Alias : tabMatch.NewName;
                caption = tabMatch.NewName;
            }
        }

        return (caption, alias);
    }

    /// <summary>
    ///  update the info section, with the new things that are in v8+ that have no equivalent in v7
    /// </summary>
    protected override void UpdateInfoSection(XElement? info, XElement target, Guid key, SyncMigrationContext context)
    {
        if (info == null) return;

        var targetInfo = info.Clone();
        if (targetInfo != null)
        {
            targetInfo.Element("Key")?.Remove();
            targetInfo.Element("Alias")?.Remove();

            targetInfo.Add(new XElement("Variations", "Nothing"));
            targetInfo.Add(new XElement("IsElement", context.ContentTypes.IsElementType(key)));

            target.Add(targetInfo);
        }
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
                contentType.Value = element!.Value;

                transformedStructure.Add(contentType);
                i++;
            }
            target.Add(transformedStructure.Clone());
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
