using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Migrators.Models.NuPickers;
using Umbraco.Extensions;

namespace uSync.Migrations.Migrators.Community
{
    [SyncMigrator("nuPickers.EnumCheckBoxPicker")]
    public class NuPickersEnumCheckBoxPickerToContentmentDataList : NuPickersToContentmentDataListBase
    {
        public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        {
            var nuPickersConfig = JsonConvert.DeserializeObject<NuPickersEnumConfig>(dataTypeProperty.PreValues?.GetPreValueOrDefault("dataSource", string.Empty));

            if (nuPickersConfig == null) return null;

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
                { key = "Umbraco.Community.Contentment.DataEditors.CheckboxListDataListEditor, Umbraco.Community.Contentment",
                    value = new
                    {
                         checkAll = "false"
                    }
                }
            }.ToList();

            var config = new JObject();

            config?.Add("dataSource", JToken.Parse(JsonConvert.SerializeObject(dataSource)));
            config?.Add("listEditor", JToken.Parse(JsonConvert.SerializeObject(listEditor)));

            return config;

        }
    }
}
