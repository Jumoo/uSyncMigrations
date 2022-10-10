using Newtonsoft.Json.Linq;

using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.DataTypes;

public class IntergerMigrator : DataTypeMigratorBase
{
    public override string[] Editors => new[] { "Umbraco.Integer", "Umbraco.Decimal" };

    public override object GetConfigValues(SyncDataTypeInfo dataTypeInfo)
    {
        var item = new JObject();

        item.AddIntPreValue(dataTypeInfo.PreValues, "min");
        item.AddDecimalPreValue(dataTypeInfo.PreValues, "step");
        item.AddIntPreValue(dataTypeInfo.PreValues, "max");

        return item;
    }
}
