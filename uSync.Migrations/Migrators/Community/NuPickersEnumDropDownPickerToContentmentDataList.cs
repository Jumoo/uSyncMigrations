using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Extensions;
using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Migrators.Models.NuPickers;

namespace uSync.Migrations.Migrators.Community;

[SyncMigrator("nuPickers.EnumDropDownPicker")]
public class NuPickersEnumDropDownPickerToContentmentDataList : NuPickersToContentmentDataListBase
{
    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var nuPickersConfig = JsonConvert.DeserializeObject<NuPickersEnumConfig>(dataTypeProperty.PreValues.GetPreValueOrDefault("dataSource", string.Empty));

        if (nuPickersConfig == null)
            return null;

        //Using an anonymous object for now, but this should be replaced with Contentment objects (when they're created).
        var dataSource = new[]
        {
            new
            { key = "Umbraco.Community.Contentment.DataEditors.EnumDataListSource, Umbraco.Community.Contentment",
                value = new
                {
                    enumType = new [] { nuPickersConfig?.AssemblyName.TrimEnd(".dll"), nuPickersConfig?.EnumName }
                }
            }
        }.ToList();

        var listEditor = new[]
        {
            new
            { key = "Umbraco.Community.Contentment.DataEditors.DropdownListDataListEditor, Umbraco.Community.Contentment",
                value = new
                {
                    allowEmpty = "false"
                }
            }
        }.ToList();

        var config = new JObject();

        config?.Add("dataSource", JToken.Parse(JsonConvert.SerializeObject(dataSource)));
        config?.Add("listEditor", JToken.Parse(JsonConvert.SerializeObject(listEditor)));

        return config;

    }
}