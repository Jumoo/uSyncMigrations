using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators;

[SyncMigrator("Umbraco.NoEdit")]
public class LabelMigrator : SyncPropertyMigratorBase
{
    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.Label;

    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        return new LabelConfiguration
        {
            ValueType = dataTypeProperty.PreValues.GetPreValueOrDefault(UmbConstants.PropertyEditors.ConfigurationKeys.DataValueType, ValueTypes.String)
        };
    }
}
