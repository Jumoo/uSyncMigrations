using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;
internal class ListViewMigrator : SyncMigratorBase
{
    public override string[] Editors => new[] { "Umbraco.ListView" };

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
        => preValues.ConvertPreValuesToJson(true);
}
