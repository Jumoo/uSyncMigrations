using uSync.Migrations.Core.Handlers;
using uSync.Migrations.Core.Plans.Models;

namespace uSync.Migrations.Core.Plans.CoreProfiles;

public class BlockListMigrationPlan : ISyncMigrationPlan
{
    private readonly SyncMigrationHandlerCollection _migrationHandlers;

    public BlockListMigrationPlan(SyncMigrationHandlerCollection migrationHandlers)
    {
        _migrationHandlers = migrationHandlers;
    }

    public int Order => 200;

    public string Name => "Convert Nested Content to BlockLists";

    public string Icon => "icon-brick color-green";

    public string Description => "Convert Nested content to BlockLists";

    public MigrationOptions Options => new MigrationOptions
    {
        Group = "Convert",
        Source = "uSync/v9",
        Target = $"{uSyncMigrations.MigrationFolder}/{DateTime.Now:yyyyMMdd_HHmmss}",
        Handlers = _migrationHandlers.SelectGroup(8, string.Empty),
        SourceVersion = 8,
        PreferredMigrators = new Dictionary<string, string>
        {
            { uSyncMigrations.EditorAliases.NestedContent, "NestedToBlockListMigrator" },
        }
    };
}
