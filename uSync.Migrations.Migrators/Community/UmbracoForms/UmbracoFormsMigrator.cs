using uSync.Migrations.Core.Extensions;

namespace uSync.Migrations.Migrators.Community.UmbracoForms;

[SyncMigrator("UmbracoForms.FormPicker")]
public class UmbracoFormsMigrator : SyncPropertyMigratorBase
{
    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => dataTypeProperty.PreValues.ConvertPreValuesToJson(true);
}
