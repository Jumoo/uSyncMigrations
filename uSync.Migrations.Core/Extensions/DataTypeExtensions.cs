using Newtonsoft.Json;
using System.Xml.Linq;
using Umbraco.Extensions;

using uSync.Core;
using uSync.Migrations.Core.Context;
using uSync.Migrations.Core.Models;

namespace uSync.Migrations.Core.Extensions;
internal static class DataTypeExtensions
{
    public static XElement MakeXMLFromNewDataType(this NewDataTypeInfo newDataType, 
        JsonSerializerSettings _jsonSerializerSettings)
    {
        var source = new XElement("DataType",
                     new XAttribute(uSyncConstants.Xml.Key, newDataType.Alias.ToGuid()),
                     new XAttribute(uSyncConstants.Xml.Alias, newDataType.Alias),
                     new XAttribute(uSyncConstants.Xml.Level, 1),
                     new XElement(uSyncConstants.Xml.Info,
                         new XElement(uSyncConstants.Xml.Name, newDataType.Name),
                         new XElement("EditorAlias", newDataType.EditorAlias),
                         new XElement("DatabaseType", newDataType.DatabaseType)));

        if (newDataType.Config != null)
        {
            source.Add(new XElement("Config",
                new XCData(JsonConvert.SerializeObject(newDataType.Config, _jsonSerializerSettings))));
        }

        return source;
    }
}
