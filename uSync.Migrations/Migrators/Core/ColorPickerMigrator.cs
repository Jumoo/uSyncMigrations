using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;
using uSync.Migrations.Context;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators;

[SyncMigrator("Umbraco.ColorPickerAlias")]
public class ColorPickerMigrator : SyncPropertyMigratorBase
{
    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.ColorPicker;

    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var config = new ColorPickerConfiguration();
        if (dataTypeProperty.PreValues == null) return config;

        int count = 0;

        foreach (var prevalue in dataTypeProperty.PreValues)
        {
            if (prevalue.Alias.InvariantEquals("useLabel"))
            {
                config.UseLabel = string.IsNullOrEmpty(prevalue.Value) ? false : int.Parse(prevalue.Value) == 1;
            }
            else
            {
                if (prevalue.Value.DetectIsJson())
                {
                    var currentValue = JsonConvert.DeserializeObject<LegacyColourValue>(prevalue.Value);
                    if (currentValue != null)
                    {
                        var newValue = new ColorItemValue
                        {
                            Label = currentValue.Label,
                            Value = currentValue.Value
                        };

                        config.Items.Add(new ValueListConfiguration.ValueListItem
                        {
                            Id = currentValue.SortOrder + 1,
                            Value = JsonConvert.SerializeObject(newValue)
                        });
                    }
                }
                else
                {
                    config.Items.Add(new ValueListConfiguration.ValueListItem
                    {
                        Id = count,
                        Value = prevalue.Value
                    });
                }
            }

            count++;
        }

        return config;
    }

    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (contentProperty.Value == null) return null;

        if (contentProperty.Value.DetectIsJson() == true)
        {
            var legacyValue = JsonConvert.DeserializeObject<ColorItemValue>(contentProperty.Value);
            if (legacyValue == null) return contentProperty.Value;

            // TODO: [KJ] Sort order is actually set in v8+ i am not sure if it is then used?
            var newValue = new ColourContentValue
            {
                SortOrder = 1,
                Id = "1",
                Label = legacyValue.Label,
                Value = legacyValue.Value
            };

            return JsonConvert.SerializeObject(newValue, Formatting.Indented);
        }
        else
        {
            return JsonConvert.SerializeObject(new ColourContentValue
            {
                SortOrder = 1,
                Id = "1",
                Label = contentProperty.Value,
                Value = contentProperty.Value
            }, Formatting.Indented);
        }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    private class ColorItemValue
    {
        public string Value { get; set; }
        public string Label { get; set; }
    }

    private class LegacyColourValue : ColorItemValue
    {
        public int SortOrder { get; set; }
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    private class ColourContentValue
    {
        public string Value { get; set; }
        public string Label { get; set; }
        public int SortOrder { get; set; }
        public string Id { get; set; }
    }
}
