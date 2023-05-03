using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;
using Umbraco.Extensions;
using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators.Community
{
    [SyncMigrator("nuPickers.XmlDropdownPicker")]
    public class NuPickersXPathDropdownPickerToContentmentDataList : NuPickersToContentmentDataListBase
    {
        public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        {
            var nuPickersConfig = JsonConvert.DeserializeObject<NuPickersConfig>(dataTypeProperty.PreValues.GetPreValueOrDefault("dataSource", string.Empty));

            if (nuPickersConfig == null) return null;

            //XPath datasource is now erroring out if the XPath starts with a // so replacing with $root as a catch all.
            if (nuPickersConfig.XPath.StartsWith("//")) nuPickersConfig.XPath = nuPickersConfig.XPath.ReplaceFirst("//", "$root/");

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

        public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
        {
            if (!string.IsNullOrWhiteSpace(contentProperty.Value))
            {
                var guid = context.GetKey(int.Parse(contentProperty.Value));
                if (guid != Guid.Empty)
                {
                    var contentUdi = Udi.Create(UmbConstants.UdiEntityType.Document, guid);
                    return contentUdi.ToString();
                }
            }

            return string.Empty;
        }
    }

    internal class NuPickersConfig
    {
        public string ApiController { get; set; }
        public string XmlData { get; set; }
        public string XPath { get; set; }
        public string KeyXPath { get; set; }
        public string LabelXPath { get; set; }
    }
}
