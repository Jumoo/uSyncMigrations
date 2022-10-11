using Newtonsoft.Json.Linq;

using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

public class IntergerMigrator : SyncMigratorBase
{
    public override string[] Editors => new[] { "Umbraco.Integer", "Umbraco.Decimal" };

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
    {
        var item = new JObject();

        item.AddIntPreValue(preValues, "min");
        item.AddDecimalPreValue(preValues, "step");
        item.AddIntPreValue(preValues, "max");

        return item;
    }
}
