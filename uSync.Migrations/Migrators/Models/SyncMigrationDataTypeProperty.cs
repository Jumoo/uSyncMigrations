using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Umbraco.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.Models;

public sealed class SyncMigrationDataTypeProperty : SyncMigrationPropertyBase
{
    public SyncMigrationDataTypeProperty(string dataTypeAlias, string editorAlias, string databaseType, IList<PreValue> preValues)
        : base(editorAlias)
    {
        DatabaseType = databaseType;
        DataTypeAlias = dataTypeAlias;
        PreValues = new ReadOnlyCollection<PreValue>(preValues);
        ConfigAsString = TranslatePreValuesToConfig(preValues);
    }

    private string? TranslatePreValuesToConfig(IList<PreValue> preValues)
    {
        var json = new Dictionary<string, object>();
        foreach (var oPreValue in preValues)
        {
            json.TryAdd(oPreValue.Alias, oPreValue.Value.ToString().DetectIsJson() ? JsonConvert.DeserializeObject(oPreValue.Value) : oPreValue);
        }
        XElement xml = new XElement("Method", 
            JsonConvert.SerializeObject(json)
            
        );
        return  xml.Value;
    }

    public SyncMigrationDataTypeProperty(string dataTypeAlias, string editorAlias, string databaseType, string? config)
        : base(editorAlias) 
    {
        DataTypeAlias = dataTypeAlias;
        DatabaseType = databaseType;
        ConfigAsString = config;
    }

    public string DataTypeAlias { get; private set; }   

    public string DatabaseType { get; private set; }

    public IReadOnlyCollection<PreValue>? PreValues { get; private set; }

    public string? ConfigAsString { get; private set; }
}