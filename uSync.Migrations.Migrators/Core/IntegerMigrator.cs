using Newtonsoft.Json.Linq;

using uSync.Migrations.Core.Extensions;

namespace uSync.Migrations.Migrators.Core;

[SyncMigrator(UmbEditors.Aliases.Integer)]
public class IntegerMigrator : SyncPropertyMigratorBase
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
