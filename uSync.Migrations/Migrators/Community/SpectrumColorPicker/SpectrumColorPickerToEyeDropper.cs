using System.Drawing;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators.Community.SpectrumColorPicker;

[SyncMigrator("Spectrum.Color.Picker")]
public class SpectrumColorPickerToEyeDropper : SyncPropertyMigratorBase
{
    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        var raw = contentProperty.Value;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return raw;
        }

        if (raw.StartsWith("#"))
        {
            return raw;
        }

        if (raw.Length == 6)
        {
            return $"#{raw}";
        }

        return raw;
    }

    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.ColorPickerEyeDropper;

    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty,
        SyncMigrationContext context)
    {
        var config = new EyeDropperColorPickerConfiguration();

        var mappings = new Dictionary<string, string>
        {
            { "enableTransparency", nameof(config.ShowAlpha) }
        };
        var palette = dataTypeProperty.PreValues?.FirstOrDefault(p => p.Alias == "palette")?.Value;
        if (!string.IsNullOrWhiteSpace(palette))
        {
            config.ShowPalette = true;
        }

        return config.MapPreValues(dataTypeProperty.PreValues, mappings);
    }
}