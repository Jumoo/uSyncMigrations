using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;
internal class SliderMigrator : SyncMigratorBase
{
    public override string[] Editors => new[] { "Umbraco.Slider" };

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
    {
        var config = new SliderConfiguration();

        var mappings = new Dictionary<string, string>
        {
            {"enableRange", nameof(SliderConfiguration.EnableRange) },
            {"precision", nameof(SliderConfiguration.StepIncrements) },
            {"InitVal1", nameof(SliderConfiguration.InitialValue)},
            {"InitVal2", nameof(SliderConfiguration.InitialValue2)},
            {"maxVal", nameof(SliderConfiguration.MaximumValue) },
            {"minVal", nameof(SliderConfiguration.MinimumValue) },
            {"step", nameof(SliderConfiguration.StepIncrements) },
        };

        return config.MapPreValues(preValues, mappings);
    }
}
