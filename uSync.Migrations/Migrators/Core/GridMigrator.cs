using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

public class GridMigrator : SyncPropertyMigratorBase
{
    public override string[] Editors => new[] { UmbConstants.PropertyEditors.Aliases.Grid };

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues, SyncMigrationContext context)
        => new GridConfiguration().MapPreValues(preValues);
}
