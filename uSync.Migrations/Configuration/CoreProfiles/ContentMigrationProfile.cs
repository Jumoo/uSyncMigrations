using uSync.Migrations.Composing;
using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Extensions;

namespace uSync.Migrations.Configuration.CoreProfiles;

public class ContentMigrationProfile : ISyncMigrationProfile
{
    private readonly SyncMigrationHandlerCollection _migrationHandlers;

    public ContentMigrationProfile(SyncMigrationHandlerCollection migrationHandlers)
    {
        _migrationHandlers = migrationHandlers;
    }

    public string Name => "Content";

    public string Icon => "icon-documents color-purple";

    public string Description => "Migrate all the content";

    public MigrationOptions Options => new MigrationOptions
    {
        Target = $"{uSyncMigrations.MigrationFolder}/{DateTime.Now:yyyyMMdd_HHmmss}",
        Handlers = _migrationHandlers
                        .Handlers
                        .Select(x => x.ToHandlerOption(x.Group == BackOffice.uSyncConstants.Groups.Content))
    };
}
