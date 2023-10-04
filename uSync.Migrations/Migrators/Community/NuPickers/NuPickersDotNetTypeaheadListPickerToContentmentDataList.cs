using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Extensions;
using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators;
using uSync.Migrations.Migrators.Community;
using uSync.Migrations.Migrators.Community.NuPickers.Models;
using uSync.Migrations.Migrators.Models;

[SyncMigrator("nuPickers.DotNetTypeaheadListPicker")]
public class NuPickersDotNetTypeaheadListPickerToContentmentDataList : NuPickersToContentmentDataListBase
{
    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty,
        SyncMigrationContext context)
    {
        var nuPickersConfig =
            JsonConvert.DeserializeObject<NuPickersDotNetTypeConfig>(
                dataTypeProperty.PreValues.GetPreValueOrDefault("dataSource", string.Empty));

        if (nuPickersConfig == null)
            return null;

        //Using an anonymous object for now, but this should be replaced with Contentment objects (when they're created).
        var dataSource = new[]
        {
            new
            {
                key =
                    $"{MapNamespace(nuPickersConfig?.ClassName)}, {MapAssembly(nuPickersConfig?.AssemblyName?.TrimEnd(".dll"))}",
                value = ""
            }
        }.ToList();

        var listEditor = new[]
        {
            new
            {
                key =
                    "Umbraco.Community.Contentment.DataEditors.DropdownListDataListEditor, Umbraco.Community.Contentment",
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

    public NuPickersDotNetTypeaheadListPickerToContentmentDataList(IOptions<NuPickerMigrationOptions> options) :
        base(options)
    {
    }
}