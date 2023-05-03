using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Migrators.Models.NuPickers;

namespace uSync.Migrations.Migrators.Community
{
    [SyncMigrator("nuPickers.SqlDropdownPicker")]
    public class NuPickersSqlDropdownPickerToContentmentDataList : NuPickersToContentmentDataListBase
    {
        public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        {
            var nuPickersConfig = JsonConvert.DeserializeObject<NuPickersSqlConfig>(dataTypeProperty.PreValues?.GetPreValueOrDefault("dataSource", string.Empty) ?? string.Empty);

            if (nuPickersConfig == null) return null;

            //Using an anonymous object for now, but this should be replaced with Contentment objects (when they're created).
            var dataSource = new[]
            {
                new
                { key = "Umbraco.Community.Contentment.DataEditors.SqlDataListSource, Umbraco.Community.Contentment",
                    value = new
                    {
                        Query = new [] {
                            nuPickersConfig?.Query,
                            nuPickersConfig?.ConnectionString
                        }
                    }
                }
            }.ToList();

            var listEditor = new[]
            {
                new
                { key = "Umbraco.Community.Contentment.DataEditors.DropdownListDataListEditor, Umbraco.Community.Contentment",
                    value = new
                    {
                        allowEmpty = "1",
                        htmlAttributes = Array.Empty<object>()
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
