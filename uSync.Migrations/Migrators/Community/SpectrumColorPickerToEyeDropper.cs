using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators.Community;
[SyncMigrator("Spectrum.Color.Picker")]

public class SpectrumColorPickerToEyeDropper : SyncPropertyMigratorBase
{
    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
        => contentProperty.Value;
    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.ColorPickerEyeDropper;
    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var config = new EyeDropperColorPickerConfiguration();

        var mappings = new Dictionary<string, string>
        {
            { "enableTransparency", nameof(config.ShowAlpha) }
        };

        return config.MapPreValues(dataTypeProperty.PreValues, mappings);
    }
}