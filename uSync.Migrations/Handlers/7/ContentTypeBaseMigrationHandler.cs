using System.Xml.Linq;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Entities;

using uSync.Core;
using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;

internal abstract class ContentTypeBaseMigrationHandler<TEntity> : MigrationHandlerBase<TEntity>
    where TEntity : IEntity
{
    public ContentTypeBaseMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService)
        : base(eventAggregator, migrationFileService)
    { }

    protected override void PrepareFile(XElement source, SyncMigrationContext context)
    {
        var contentTypeAlias = source.Element("Info")?.Element("Alias")?.ValueOrDefault(string.Empty) ?? string.Empty;

        context.AddContentTypeKey(contentTypeAlias, source.Element("Info")?.Element("Key")?.ValueOrDefault(Guid.Empty));

        var compositions = source.Element("Info")?.Element("Compositions")?.Elements("Composition")?.Select(x => x.Value) ?? Enumerable.Empty<string>();
        context.AddContentTypeCompositions(contentTypeAlias, compositions);

        var properties = source.Element("GenericProperties")?.Elements("GenericProperty") ?? Enumerable.Empty<XElement>();

        foreach (var property in properties)
        {
            var editorAlias = property.Element("Type").ValueOrDefault(string.Empty);
            var definition = property.Element("Definition").ValueOrDefault(Guid.Empty);
            var alias = property.Element("Alias")?.ValueOrDefault(string.Empty) ?? string.Empty;

            context.AddContentProperty(contentTypeAlias, alias,
                    editorAlias, context.GetDataTypeFromDefinition(definition));

            //
            // for now we are doing this just for media folders, but it might be
            // that all list view properties should be ignored ??
            if (contentTypeAlias.Equals("Folder") && editorAlias.Equals("Umbraco.ListView"))
            {
                context.AddIgnoredProperty(contentTypeAlias, alias);
            }
        }
    }

    protected override XElement? MigrateFile(XElement source, int level, SyncMigrationContext context)
    {
        var info = source.Element("Info");
        if (info == null) return null;

        var key = info.Element("Key").ValueOrDefault(Guid.Empty);
        var alias = info.Element("Alias").ValueOrDefault(string.Empty);

        if (context.IsBlocked(ItemType, alias)) return null;

        var target = new XElement(ItemType,
            new XAttribute(uSyncConstants.Xml.Key, key),
            new XAttribute(uSyncConstants.Xml.Alias, alias),
            new XAttribute(uSyncConstants.Xml.Level, level));

        // update info element. 
        ContentTypeBaseMigrationHandler<TEntity>.UpdateInfoSection(info, target, key, context);

        // structure
        UpdateStructure(source, target);

        // properties. 
        ContentTypeBaseMigrationHandler<TEntity>.UpdateProperties(source, target, alias, context);

        // tabs
        UpdateTabs(source, target);

        ContentTypeBaseMigrationHandler<TEntity>.CheckVariations(target);

        return target;
    }

    private static void UpdateTabs(XElement source, XElement target)
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

    private static void UpdateProperties(
        XElement source,
        XElement target,
        string contentTypeAlias,
        SyncMigrationContext context)
    {
        var properties = source.Element("GenericProperties");

        var newProperties = new XElement("GenericProperties");
        if (properties != null)
        {
            foreach (var property in properties.Elements("GenericProperty"))
            {
                var name = property.Element("Name").ValueOrDefault(string.Empty);

                if (context.IsIgnoredProperty(contentTypeAlias, name))
                {
                    continue;
                }

                var newProperty = XElement.Parse(property.ToString());

                // update the datatype we are using (this might be new). 
                ContentTypeBaseMigrationHandler<TEntity>.UpdatePropertyEditor(contentTypeAlias, newProperty, context);

                newProperty.Add(new XElement("MandatoryMessage", string.Empty));
                newProperty.Add(new XElement("ValidationRegExpMessage", string.Empty));
                newProperty.Add(new XElement("LabelOnTop", false));

                var tabNode = newProperty.Element("Tab");               
                tabNode?.Add(new XAttribute("Alias", tabNode.ValueOrDefault(string.Empty)));

                newProperties.Add(newProperty);
            }
        }

        target.Add(newProperties);
    }

    /// <summary>
    ///  Get the editor Alias for this property (it might have updated)
    /// </summary>
    /// <param name="newProperty"></param>
    private static void UpdatePropertyEditor(string contentTypeAlias, XElement newProperty, SyncMigrationContext context)
    {
        var propertyAlias = newProperty.Element("Alias").ValueOrDefault(string.Empty);

        var updatedType = context.GetEditorAlias(contentTypeAlias, propertyAlias)?.UpdatedEditorAlias ?? propertyAlias;
        newProperty.CreateOrSetElement("Type", updatedType);

        var definitionElement = newProperty.Element("Definition");
        if (definitionElement == null) return;

        var definition = definitionElement.ValueOrDefault(Guid.Empty);
        if (definition != Guid.Empty)
        {
            definitionElement.Value = context.GetReplacementDataType(definition).ToString();
            newProperty.CreateOrSetElement("Variations", context.GetDataTypeVariation(definition));
        }
        else
        {
            newProperty.CreateOrSetElement("Variations", "Nothing");
        }
    }

    /// <summary>
    ///  update the info section, with the new things that are in v8+ that have no equivalent in v7
    /// </summary>
    private static void UpdateInfoSection(XElement? info, XElement target, Guid key, SyncMigrationContext context)
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
    private static void UpdateStructure(XElement source, XElement target)
    {
        var sourceStructure = source.Element("Structure");
        if (sourceStructure != null)
            target.Add(XElement.Parse(sourceStructure.ToString()));
    }

    private static void CheckVariations(XElement target)
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
