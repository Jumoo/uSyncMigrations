using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Core.Extensions;

namespace uSync.Migrations.Migrators.Core;

[SyncMigrator("Umbraco.NoEdit")]
public class LabelMigrator : SyncPropertyMigratorBase
{
    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => UmbEditors.Aliases.Label;

    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        return new LabelConfiguration
        {
            ValueType = dataTypeProperty.PreValues.GetPreValueOrDefault(UmbEditors.ConfigurationKeys.DataValueType, ValueTypes.String)
        };
    }
}
