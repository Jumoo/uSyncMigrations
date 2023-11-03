using System.Xml.Serialization;

using Microsoft.Extensions.Options;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

using Umbraco.Cms.Core;
using Umbraco.Extensions;

using uSync.Migrations.Core.Extensions;

namespace uSync.Migrations.Migrators.Community.NuPickers;

[SyncMigrator("nuPickers.XmlDropdownPicker")]
public class NuPickersXPathDropdownPickerToContentmentDataList : NuPickersToContentmentDataListBase
{
    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var nuPickersConfig = JsonConvert.DeserializeObject<NuPickersConfig>(dataTypeProperty.PreValues.GetPreValueOrDefault("dataSource", string.Empty));

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
            // The XPath DataList Dropdown can only pick one value
            // but it isn't limited to being the Integer Node Id, it can be configured to be any property
            // similarly it's storage isn't guaranteed to be as an integer, it can be CSV, JSON, XML or Relations Only!
            // without knowing what has been configured (and I understand it's difficult to know from the uSync Content files, but kev is going to have a look at it)
            // we can only really guess based on the value.

            // first let's see if it's Json, and if it is deserialise into a string
            // then look for XML
            // then fall back to TryParse with Int... maybe this logic will be useful in other NuPickers... but they might store multiple values...

            // Is it JSON?
            string valueToParse = string.Empty;
            if (contentProperty.Value.DetectIsJson())
            {
                var nuPickerValues = JsonConvert.DeserializeObject<IEnumerable<NuPickerValue>>(contentProperty.Value);
                if (nuPickerValues != null)
                {
                    var nuPickerValue = nuPickerValues.FirstOrDefault();
                    if (nuPickerValue?.key != null)
                    {
                        valueToParse = nuPickerValue.key;
                    }
                }
            }
            else if (contentProperty.Value.Contains("<Picker>"))
            {
                // then this is XML storage
                XmlSerializer serializer = new XmlSerializer(typeof(Picker));
                Picker? picker;

                using (TextReader reader = new StringReader(contentProperty.Value))
                {
                    picker = (Picker?)serializer.Deserialize(reader);
                }
                valueToParse = picker?.PickedItems?.FirstOrDefault()?.Key ?? string.Empty;
            }
            else
            {
                // it is a string or an integer as a string
                valueToParse = contentProperty.Value;
            }

            //this will only work if the NuPicker is storing an nodeId

            if (int.TryParse(valueToParse, out int nodeId))
            {
                var guid = context.GetKey(nodeId);
                if (guid != Guid.Empty)
                {
                    var contentUdi = Udi.Create(UmbConstants.UdiEntityType.Document, guid);
                    return contentUdi.ToString();
                }
            }
        }

        return string.Empty;
    }

    public NuPickersXPathDropdownPickerToContentmentDataList(IOptions<NuPickerMigrationOptions> options) : base(options)
    {
    }
}

internal class NuPickerValue
{
    [XmlElement("key")]
    public string? key { get; set; }
    [XmlElement("label")]
    public string? label { get; set; }
}

[XmlRoot("Picker")]
internal class Picker
{
    [XmlElement("Picked")]
    public List<Picked>? PickedItems { get; set; }

}

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
internal class Picked
{
    [XmlAttribute("Key")]
    public string? Key { get; set; }

    [XmlText]
    public string? Label { get; set; }
}

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
internal class NuPickersConfig
{
    public string? ApiController { get; set; }
    public string? XmlData { get; set; }
    public string? XPath { get; set; }
    public string? KeyXPath { get; set; }
    public string? LabelXPath { get; set; }
}
