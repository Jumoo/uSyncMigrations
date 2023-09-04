using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Context;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.DateTime, typeof(DateTimeConfiguration), IsDefaultAlias = true)]
[SyncMigrator("Umbraco.Date")]
public class DatePickerMigrator : SyncPropertyMigratorBase
{ 
    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.DateTime;
}
