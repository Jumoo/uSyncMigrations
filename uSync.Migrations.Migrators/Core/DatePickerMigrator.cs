using Umbraco.Cms.Core.PropertyEditors;

namespace uSync.Migrations.Migrators.Core;

[SyncMigrator(UmbEditors.Aliases.DateTime, typeof(DateTimeConfiguration), IsDefaultAlias = true)]
[SyncMigrator("Umbraco.Date")]
public class DatePickerMigrator : SyncPropertyMigratorBase
{
    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => UmbEditors.Aliases.DateTime;
}
