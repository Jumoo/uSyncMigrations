using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.Community;

[SyncMigrator("UmbracoForms.FormPicker")]
internal class UmbracoFormsMigrator : SyncPropertyMigratorBase
{
    public override object GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => dataTypeProperty.PreValues.ConvertPreValuesToJson(true);
}
