using System.Xml.Linq;

using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

using uSync.Core;
using uSync.Migrations.Models;

namespace uSync.Migrations.Extensions;
internal static class ContentTypeExtensions
{
	public static XElement MakeXMLFromNewDocType(this NewContentTypeInfo newDocType,
		IDataTypeService dataTypeService)
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
						 new XElement("Folder", newDocType.Folder ?? "")),
					 new XElement("GenericProperties"),
					 new XElement("Tabs",
						 new XElement("Tab",
							 new XElement("Key", Guid.NewGuid().ToString()),
							 new XElement("Caption", "Block"),
							 new XElement("Alias", "block"),
							 new XElement("Type", "Group"),
							 new XElement("SortOrder", 0))
						 )
					 );

		var properties = source.Element("GenericProperties");
		if (properties != null)
		{
			var index = 0;
			foreach (var property in newDocType.Properties)
			{
				index++;

				var dataType = dataTypeService.GetDataType(property.DataTypeAlias);
				if (dataType == null) continue;

				var propNode = new XElement("GenericProperty",
				new XElement("Key", $"{newDocType.Alias}_{property.Alias}".ToGuid()),
					new XElement("Name", property.Name),
					new XElement("Alias", property.Alias),
					new XElement("Definition", dataType.Key),
					new XElement("Type", dataType.EditorAlias),
					new XElement("Mandatory", false),
					new XElement("Validation", ""),
					new XElement("Description", new XCData("")),
					new XElement("SortOrder", index),
					new XElement("Tab", "Block", new XAttribute("Alias", "block")),
					new XElement("Variations", "Nothing"),
					new XElement("MandatoryMessage", ""),
					new XElement("ValidationRegExpMessage", ""),
					new XElement("LabelOnTop", ""));

				properties.Add(propNode);
			}
		}

		return source;


	}
}
