using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Core.Extensions;

namespace uSync.Migrations.Migrators.Core;

[SyncMigrator(UmbEditors.Aliases.Slider)]
public class SliderMigrator : SyncPropertyMigratorBase
{
    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var config = new SliderConfiguration();

        var mappings = new Dictionary<string, string>
        {
            { "enableRange", nameof(SliderConfiguration.EnableRange) },
            { "precision", nameof(SliderConfiguration.StepIncrements) },
            { "InitVal1", nameof(SliderConfiguration.InitialValue)},
            { "InitVal2", nameof(SliderConfiguration.InitialValue2)},
            { "maxVal", nameof(SliderConfiguration.MaximumValue) },
            { "minVal", nameof(SliderConfiguration.MinimumValue) },
            { "step", nameof(SliderConfiguration.StepIncrements) },
        };

        return config.MapPreValues(dataTypeProperty.PreValues, mappings);
    }
}
