using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;

internal class MediaTypeMigrationHandler : ContentTypeBaseMigrationHandler, ISyncMigrationHandler
{
    public MediaTypeMigrationHandler(
        MigrationFileService migrationFileService,
        SyncMigratorCollection migrators) 
        : base(migrationFileService, migrators, "MediaType")
    { }

    public int Priority => uSyncMigrations.Priorities.MediaTypes;

    public IEnumerable<MigrationMessage> MigrateFromDisk(Guid migrationId, string sourceFolder, MigrationContext context)
    {
        return base.MigrateFromDisk(
            migrationId, Path.Combine(sourceFolder, "MediaType"), "MediaType", "MediaTypes", context);
    }
    public void PrepMigrations(Guid migrationId, string sourceFolder, MigrationContext context)
    {
        PrepContext(Path.Combine(sourceFolder, "MediaType"), context);
    }
}
