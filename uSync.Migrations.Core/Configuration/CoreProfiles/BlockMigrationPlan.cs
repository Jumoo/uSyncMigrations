using uSync.Migrations.Core.Composing;
using uSync.Migrations.Core.Configuration.Models;

namespace uSync.Migrations.Core.Configuration.CoreProfiles;

public class BlockMigrationPlan : ISyncMigrationPlan
{
    private readonly SyncMigrationHandlerCollection _migrationHandlers;

    public BlockMigrationPlan(SyncMigrationHandlerCollection migrationHandlers)
    {
        _migrationHandlers = migrationHandlers;
    }

    public int Order => 200;

    public string Name => "Convert Nested Content to BlockList and Grid to BlockGrid";

    public string Icon => "icon-brick color-green";

    public string Description => "Convert Nested content and Grid to BlockList and BlockGrid  (Experimental!)";

    public MigrationOptions Options => new MigrationOptions
    {
        Group = "Convert",
        Source = "uSync/v9",
        Target = $"{uSyncMigrations.MigrationFolder}/{DateTime.Now:yyyyMMdd_HHmmss}",
        Handlers = _migrationHandlers.SelectGroup(8, string.Empty),
        SourceVersion = 8,
        PreferredMigrators = new Dictionary<string, string>
        {
            { UmbConstants.PropertyEditors.Aliases.NestedContent, "NestedToBlockListMigrator" },
            { UmbConstants.PropertyEditors.Aliases.Grid, "GridToBlockGridMigrator" }
        }
    };
}
