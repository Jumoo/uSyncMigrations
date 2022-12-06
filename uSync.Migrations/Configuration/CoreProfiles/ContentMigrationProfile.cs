using uSync.Migrations.Composing;
using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Extensions;

namespace uSync.Migrations.Configuration.CoreProfiles;

public class ContentMigrationProfile : ISyncMigrationProfile
{
    public int Order => 110;

    private readonly SyncMigrationHandlerCollection _migrationHandlers;
    private readonly SyncPropertyMigratorCollection _migrators;

    public ContentMigrationProfile(
        SyncMigrationHandlerCollection migrationHandlers,
        SyncPropertyMigratorCollection migrators)
    {
        _migrationHandlers = migrationHandlers;
        _migrators = migrators;
    }

    public string Name => "Content";

    public string Icon => "icon-documents color-purple";

    public string Description => "Migrate all the content";

    public MigrationOptions Options => new MigrationOptions
    {
        Target = $"{uSyncMigrations.MigrationFolder}/{DateTime.Now:yyyyMMdd_HHmmss}",
        Handlers = _migrationHandlers.SelectGroup(BackOffice.uSyncConstants.Groups.Content),
    };
}
