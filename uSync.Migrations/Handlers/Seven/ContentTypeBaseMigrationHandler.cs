using System.Xml.Linq;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

using uSync.Core;
using uSync.Migrations.Handlers.Shared;
using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Seven;

internal abstract class ContentTypeBaseMigrationHandler<TEntity> : SharedContentTypeBaseHandler<TEntity>
    where TEntity : ContentTypeBase
{
    public ContentTypeBaseMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService)
        : base(eventAggregator, migrationFileService)
    { }

    protected override (string alias, Guid key) GetAliasAndKey(XElement source)
        => (
            alias: source.Element("Info")?.Element("Alias")?.ValueOrDefault(string.Empty) ?? string.Empty,
            key: source.Element("Info")?.Element("Key")?.ValueOrDefault(Guid.Empty) ?? Guid.Empty
        );

    protected override void UpdateTabs(XElement source, XElement target)
    {
        var tabs = source.Element("Tabs");
        if (tabs != null)
        {
            var newTabs = new XElement("Tabs");
            foreach (var tab in tabs.Elements("Tab"))
            {
                var newTab = XElement.Parse(tab.ToString());
                newTab.Add(new XElement("Alias", tab.Element("Caption").ValueOrDefault(string.Empty)));
                newTab.Add(new XElement("Type", "Group"));
                newTabs.Add(newTab);
            }
            target.Add(newTabs);
        }
    }

    protected override void UpdatePropertyXml(XElement newProperty)
    {
        newProperty.Add(new XElement("MandatoryMessage", string.Empty));
        newProperty.Add(new XElement("ValidationRegExpMessage", string.Empty));
        newProperty.Add(new XElement("LabelOnTop", false));

        var tabNode = newProperty.Element("Tab");
        tabNode?.Add(new XAttribute("Alias", tabNode.ValueOrDefault(string.Empty)));
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
        targetInfo.Add(new XElement("IsElement", context.IsElementType(key)));

        target.Add(targetInfo);
    }

    /// <summary>
    ///  update the structure (allowed nodes)
    /// </summary>
    protected override void UpdateStructure(XElement source, XElement target)
    {
        var sourceStructure = source.Element("Structure");
        if (sourceStructure != null)
            target.Add(XElement.Parse(sourceStructure.ToString()));
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
