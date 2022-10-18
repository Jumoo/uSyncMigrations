using Newtonsoft.Json.Linq;
using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

public class IntegerMigrator : SyncPropertyMigratorBase
{
    public override string[] Editors => new[] { UmbConstants.PropertyEditors.Aliases.Integer };

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues, SyncMigrationContext context)
    {
        var item = new JObject();

        item.AddIntPreValue(preValues, "min");
        item.AddDecimalPreValue(preValues, "step");
        item.AddIntPreValue(preValues, "max");

        return item;
    }
}
