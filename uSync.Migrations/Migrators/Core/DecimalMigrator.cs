using Newtonsoft.Json.Linq;
using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.Decimal)]
public class DecimalMigrator : SyncPropertyMigratorBase
{
    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var item = new JObject();

        item.AddIntPreValue(dataTypeProperty.PreValues, "min");
        item.AddDecimalPreValue(dataTypeProperty.PreValues, "step");
        item.AddIntPreValue(dataTypeProperty.PreValues, "max");

        return item;
    }
}
