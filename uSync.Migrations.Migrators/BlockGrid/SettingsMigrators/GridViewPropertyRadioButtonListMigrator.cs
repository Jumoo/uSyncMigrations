using Newtonsoft.Json;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;
using uSync.Migrations.Migrators.BlockGrid.Models;

namespace uSync.Migrations.Migrators.BlockGrid.SettingsMigrators;

public class GridViewPropertyRadioButtonListMigrator : IGridSettingsViewMigrator
{
    public string ViewKey => "RadioButtonList";

    public string GetNewDataTypeAlias(string gridAlias, string? configItemLabel)
    {
        string newDataTypeAlias = gridAlias;
        if (!string.IsNullOrEmpty(configItemLabel))
        {
            newDataTypeAlias += " - " + configItemLabel;
        }
        newDataTypeAlias += " - Radio Button List";

        return newDataTypeAlias;
    }


    public object ConvertContentString(string value)
    {
        return value;
    }

    public NewDataTypeInfo? GetAdditionalDataType(string dataTypeAlias, IEnumerable<GridSettingsConfigurationItemPrevalue>? preValues)
    {
        if (preValues is null)
        {
            return null;
        }

        NewDataTypeInfo newDataTypeInfo = 
            new NewDataTypeInfo(dataTypeAlias.ToGuid(), 
                                dataTypeAlias, 
                                dataTypeAlias, 
                                "Umbraco.RadioButtonList", 
                                nameof(ValueStorageType.Nvarchar), 
                                new RadioButtonListConfig(preValues));

        return newDataTypeInfo;
    }
}

public class RadioButtonListConfig
{
    public RadioButtonListConfig(IEnumerable<GridSettingsConfigurationItemPrevalue> preValues) 
    {
        Items = preValues.Select((preValue, ix) => new RadioButtonListConfigItem() { Id = ix, Value = preValue.Label, OldValue = preValue.Value });
    }

    [JsonProperty("items")]
    public IEnumerable<RadioButtonListConfigItem> Items { get; set; }
}

public class RadioButtonListConfigItem
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("value")]
    public string? Value { get; set; }

    [JsonIgnore]
    public string? OldValue { get; set; }
}