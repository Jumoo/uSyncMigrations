using uSync.Migrations.Composing;
using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Migrators.Optional;

namespace uSync.Migrations.Configuration.CoreProfiles;

public class BlockGridMigrationPlan : ISyncMigrationPlan
{
    private readonly SyncMigrationHandlerCollection _migrationHandlers;

    public BlockGridMigrationPlan(SyncMigrationHandlerCollection migrationHandlers)
    {
        _migrationHandlers = migrationHandlers;
    }

    public int Order => 200;

    public string Name => "Convert Grid to BlockGrid";

    public string Icon => "icon-brick color-green";

    public string Description => "Convert Grid to BlockGrid";

    public MigrationOptions Options => new MigrationOptions
    {
        Group = "Convert",
        Source = "uSync/v9",
        Target = $"{uSyncMigrations.MigrationFolder}/{DateTime.Now:yyyyMMdd_HHmmss}",
        Handlers = _migrationHandlers.SelectGroup(8, string.Empty),
        SourceVersion = 8,
        PreferredMigrators = new Dictionary<string, string>
        {
            { UmbConstants.PropertyEditors.Aliases.NestedContent, nameof(NestedToBlockListMigrator) },
        }
    };
}
