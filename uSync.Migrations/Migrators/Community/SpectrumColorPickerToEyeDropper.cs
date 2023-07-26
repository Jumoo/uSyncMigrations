using System.Drawing;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators.Community;

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

        if (raw.Contains("rgb"))
        {
            var colourMatches = Regex.Match(raw,
                @"(rgba|rgb)\(\s?(\d{1,3})\,\s?(\d{1,3})\,\s?(\d{1,3})(\,\s?(\d|\d\.\d+))?\s?\)");

            if (colourMatches.Groups.Count < 5) return raw;
            var color = Color.FromArgb(
                (int)Math.Round((double.TryParse(colourMatches.Groups[4]?.ToString(), out double parsed)
                    ? parsed
                    : (double)1) * 255), int.Parse(colourMatches.Groups[2].ToString()),
                int.Parse(colourMatches.Groups[3].ToString()), int.Parse(colourMatches.Groups[4].ToString()));
            return ColorTranslator.ToHtml(color);
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
        var palet = dataTypeProperty.PreValues?.FirstOrDefault(p => p.Alias == "palette")?.Value;
        if (!string.IsNullOrWhiteSpace(palet))
        {
            config.ShowPalette = true;
        }

        return config.MapPreValues(dataTypeProperty.PreValues, mappings);
    }
}