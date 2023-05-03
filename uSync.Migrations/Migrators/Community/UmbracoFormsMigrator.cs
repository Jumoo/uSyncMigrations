using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators.Community;

[SyncMigrator("UmbracoForms.FormPicker")]
public class UmbracoFormsMigrator : SyncPropertyMigratorBase
{
    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => dataTypeProperty.PreValues.ConvertPreValuesToJson(true);
}
