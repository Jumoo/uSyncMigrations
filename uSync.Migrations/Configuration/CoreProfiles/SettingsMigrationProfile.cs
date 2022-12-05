using uSync.Migrations.Composing;
using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Extensions;

namespace uSync.Migrations.Configuration.CoreProfiles;

public class SettingsMigrationProfile : ISyncMigrationProfile
{
    public int Order => 101;

    private readonly SyncMigrationHandlerCollection _migrationHandlers;
    private readonly SyncPropertyMigratorCollection _migrators;

    public SettingsMigrationProfile(
        SyncMigrationHandlerCollection migrationHandlers,
        SyncPropertyMigratorCollection migrators)
    {
        _migrationHandlers = migrationHandlers;
        _migrators = migrators;
    }

    public string Name => "Settings";

    public string Icon => "icon-settings-alt color-blue";

    public string Description => "Migrate all the settings";

    public MigrationOptions Options => new MigrationOptions
    {
        Target = $"{uSyncMigrations.MigrationFolder}/{DateTime.Now:yyyyMMdd_HHmmss}",
        Handlers = _migrationHandlers.SelectGroup(BackOffice.uSyncConstants.Groups.Settings)
    };
}
