using uSync.Migrations.Composing;
using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Migrators.BlockGrid;
using uSync.Migrations.Migrators.Optional;

namespace uSync.Migrations.Configuration.CoreProfiles;

public class FullSevenToElevenAllTheThingsPlan : ISyncMigrationPlan
{
    private readonly SyncMigrationHandlerCollection _migrationHandlers;

    public FullSevenToElevenAllTheThingsPlan(SyncMigrationHandlerCollection migrationHandlers)
    {
        _migrationHandlers = migrationHandlers;
    }

    public int Order => 300;

    public string Name => "One step upgrade (blocklist, block grid)";

    public string Icon => "icon-bang";

    public string Description => "Does everything at once, upgrades all the things and converts" +
        "nested content to BlockList and Grids to BlockGrids";

    public MigrationOptions Options => new MigrationOptions
    {
        Target = $"{uSyncMigrations.MigrationFolder}/{DateTime.Now:yyyyMMdd_HHmmss}",
        Handlers = _migrationHandlers.SelectGroup(7, string.Empty),
        SourceVersion = 7,
        PreferredMigrators = new Dictionary<string, string>
        {
            { UmbConstants.PropertyEditors.Aliases.NestedContent, nameof(NestedToBlockListMigrator) },
            { UmbConstants.PropertyEditors.Aliases.Grid, nameof(GridToBlockGridMigrator) }
        }
    };
}
