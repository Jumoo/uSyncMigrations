using uSync.Migrations.Composing;
using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Extensions;

namespace uSync.Migrations.Configuration.CoreProfiles;

public class EverythingMigrationProfile : ISyncMigrationProfile
{
    public int Order => 120;

    private readonly SyncMigrationHandlerCollection _migrationHandlers;
    private readonly SyncPropertyMigratorCollection _migrators;

    public EverythingMigrationProfile(
        SyncMigrationHandlerCollection migrationHandlers,
        SyncPropertyMigratorCollection migrators)
    {
        _migrationHandlers = migrationHandlers;
        _migrators = migrators;
    }

    public string Name => "Everything";

    public string Icon => "icon-paper-plane color-orange";

    public string Description => "Migrate everything";

    public MigrationOptions Options => new MigrationOptions
    {
        Target = $"{uSyncMigrations.MigrationFolder}/{DateTime.Now:yyyyMMdd_HHmmss}",
        Migrators = _migrators.GetDefaultMigrators(),
        Handlers = _migrationHandlers.SelectGroup("")
    };
}