using Umbraco.Cms.Core.Composing;

using uSync.Migrations.Composing;
using uSync.Migrations.Configuration.Models;

namespace uSync.Migrations.Configuration.CoreProfiles;

[HideFromTypeFinder]
internal class BlockMigrationProfile : ISyncMigrationProfile
{
    private readonly SyncMigrationHandlerCollection _migrationHandlers;

    public BlockMigrationProfile(SyncMigrationHandlerCollection migrationHandlers)
    {
        _migrationHandlers = migrationHandlers;
    }

    public int Order => 200;

    public string Name => "To Blocks";

    public string Icon => "icon-block color-green";

    public string Description => "Convert to block list and block grids";

    public MigrationOptions Options => new MigrationOptions
    {
        Source = "uSync/v9",
        Target = $"{uSyncMigrations.MigrationFolder}/{DateTime.Now:yyyyMMdd_HHmmss}",
        Handlers = _migrationHandlers.SelectGroup(8, string.Empty),
        SourceVersion = 8
    };

}
