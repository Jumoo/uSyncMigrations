using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

public class GridMigrator : SyncMigratorBase
{
    public override string[] Editors => new[] { "Umbraco.Grid" };

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
        => new GridConfiguration().MapPreValues(preValues);
}
