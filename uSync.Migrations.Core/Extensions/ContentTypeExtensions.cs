using System.Xml.Linq;

using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

using uSync.Core;
using uSync.Migrations.Core.Context;
using uSync.Migrations.Core.Models;

namespace uSync.Migrations.Core.Extensions;
internal static class ContentTypeExtensions
{
    public static XElement MakeXMLFromNewDocType(this NewContentTypeInfo newDocType,
        IDataTypeService dataTypeService, SyncMigrationContext context)
    {
        var source = new XElement("ContentType",
                     new XAttribute(uSyncConstants.Xml.Key, newDocType.Alias.ToGuid()),
                     new XAttribute(uSyncConstants.Xml.Alias, newDocType.Alias),
                     new XAttribute(uSyncConstants.Xml.Level, 1),
                     new XElement(uSyncConstants.Xml.Info,
                         new XElement(uSyncConstants.Xml.Name, newDocType.Name),
                         new XElement("Icon", newDocType.Icon),
                         new XElement("Thumbnail", "folder.png"),
                         new XElement("Description", newDocType.Description),
                         new XElement("AllowAtRoot", false),
                         new XElement("IsListView", false),
                         new XElement("Variations", "Nothing"),
                         new XElement("IsElement", newDocType.IsElement),
                         new XElement("Folder", newDocType.Folder ?? ""),
                         new XElement("Compositions", GetCompositions(newDocType, context))),
                     new XElement("GenericProperties"),
                     new XElement("Tabs", GetTabs(newDocType))
                     );

        var properties = source.Element("GenericProperties");
        if (properties != null)
        {
            var index = 0;
            foreach (var property in newDocType.Properties)
            {
                index++;

                var dataType = dataTypeService.GetDataType(property.DataTypeAlias);
                var newDataType = dataType == null ?
                                  context.DataTypes.GetNewDataType(property.DataTypeAlias) :
                                  null;

                if (dataType == null && newDataType == null) continue;

                var propNode = new XElement("GenericProperty",
                new XElement("Key", $"{newDocType.Alias}_{property.Alias}".ToGuid()),
                    new XElement("Name", property.Name),
                    new XElement("Alias", property.Alias),
                    new XElement("Definition", dataType == null ? newDataType!.Key : dataType.Key),
                    new XElement("Type", dataType == null ? newDataType!.EditorAlias : dataType.EditorAlias),
                    new XElement("Mandatory", false),
                    new XElement("Validation", ""),
                    new XElement("Description", new XCData(property.Description ?? "")),
                    new XElement("SortOrder", index),
                    GetTabElement(property, newDocType),
                    new XElement("Variations", "Nothing"),
                    new XElement("MandatoryMessage", ""),
                    new XElement("ValidationRegExpMessage", ""),
                    new XElement("LabelOnTop", ""));

                properties.Add(propNode);
            }
        }

        return source;


    }

    private static XElement GetTabElement(NewContentTypeProperty property, NewContentTypeInfo contentTypeInfo)
    {
        var tab = contentTypeInfo.Tabs.FirstOrDefault(x => x.Alias == property.TabAlias);
        return tab == null ?
            new XElement("Tab", "Block", new XAttribute("Alias", "block")) :
            new XElement("Tab", tab.Name, new XAttribute("Alias", tab.Alias));
    }

    private static IEnumerable<XElement> GetCompositions(NewContentTypeInfo newDocType,
        SyncMigrationContext context)
    {
        foreach (var composition in newDocType.CompositionAliases)
        {
            var element = new XElement("Composition");
            element.Add(new XAttribute("Key", context.ContentTypes.GetKeyByAlias(composition)));
            element.Add(composition);
            yield return element;
        }
    }

    private static IEnumerable<XElement> GetTabs(NewContentTypeInfo newDocType)
    {
        foreach (var tab in newDocType.Tabs)
        {
            var element = new XElement("Tab",
                new XElement("Key", Guid.NewGuid().ToString()),
                new XElement("Caption", tab.Name),
                new XElement("Alias", tab.Alias),
                new XElement("Type", tab.Type),
                new XElement("SortOrder", tab.SortOrder));
            yield return element;
        }
    }
}
