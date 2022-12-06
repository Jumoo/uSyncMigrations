using uSync.Migrations.Composing;
using uSync.Migrations.Configuration.Models;

namespace uSync.Migrations.Configuration.CoreProfiles;

public class EverythingMigrationProfile : ISyncMigrationProfile
{
    public int Order => 120;

    private readonly SyncMigrationHandlerCollection _migrationHandlers;

    public EverythingMigrationProfile(
        SyncMigrationHandlerCollection migrationHandlers)
    {
        _migrationHandlers = migrationHandlers;
    }

    public string Name => "Everything";

    public string Icon => "icon-paper-plane color-orange";

    public string Description => "Migrate everything";

    public MigrationOptions Options => new MigrationOptions
    {
        Target = $"{uSyncMigrations.MigrationFolder}/{DateTime.Now:yyyyMMdd_HHmmss}",
        Handlers = _migrationHandlers.SelectGroup(7, string.Empty)
    };
}