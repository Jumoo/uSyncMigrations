using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Community.NuPickers.Models;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators.Community
{
    [SyncMigrator("nuPickers.XmlCheckBoxPicker")]
    public class NuPickersXPathCheckboxPickerToContentmentDataList : NuPickersToContentmentDataListBase
    {
        public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        {
            var nuPickersConfig = JsonConvert.DeserializeObject<NuPickersXmlConfig>(dataTypeProperty.PreValues.GetPreValueOrDefault("dataSource", string.Empty));

            if (nuPickersConfig == null) return null;

            // replace non-standard token '$ancestorOrSelf' parsed by nuPickers, into a Contentment '$current' placeholder token
            nuPickersConfig.XPath = nuPickersConfig.XPath?.Replace("$ancestorOrSelf", "$current") ?? null;

            //Using an anonymous object for now, but ideally this should be replaced with Contentment objects (when they're created).
            var dataSource = new[]
            {
                new
                { key = "Umbraco.Community.Contentment.DataEditors.UmbracoContentXPathDataListSource, Umbraco.Community.Contentment",
                    value = new
                    {
                        xpath = nuPickersConfig?.XPath
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

        public NuPickersXPathCheckboxPickerToContentmentDataList(IOptions<NuPickerMigrationOptions> options) : base(options)
        {
        }
    }
}
