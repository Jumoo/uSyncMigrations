using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators;

[SyncMigrator("Our.Umbraco.Switcher")]
public class SwitcherToTrueFalseMigrator : SyncPropertyMigratorBase
{
    public override string GetEditorAlias(SyncMigrationDataTypeProperty propertyModel, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.Boolean;

    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        return new TrueFalseConfiguration().MapPreValues(dataTypeProperty.PreValues, new Dictionary<string, string>
        {
            { "hideLabel", nameof(TrueFalseConfiguration.ShowLabels) },
            { "onLabelText", nameof(TrueFalseConfiguration.LabelOn) },
            { "offLabelText", nameof(TrueFalseConfiguration.LabelOff) },
            { "switchOn", nameof(TrueFalseConfiguration.Default) }
        });
    }
}