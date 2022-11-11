using Umbraco.Cms.Core.PropertyEditors;

namespace uSync.Migrations.Migrators;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.Grid, typeof(GridConfiguration))]
public class GridMigrator : SyncPropertyMigratorBase
{ }