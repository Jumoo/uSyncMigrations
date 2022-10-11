using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;

internal class ContentTypeMigrationHandler : ContentTypeBaseMigrationHandler, ISyncMigrationHandler
{
    public ContentTypeMigrationHandler(
        MigrationFileService migrationFileService,
        SyncMigratorCollection migrators)
        : base(migrationFileService,migrators, "ContentType")
    { }

    public int Priority => 20;

    public IEnumerable<MigrationMessage> MigrateFromDisk(Guid migrationId, string sourceFolder, MigrationContext context)
    {
        return base.MigrateFromDisk(
            migrationId, Path.Combine(sourceFolder, "DocumentType"), "ContentType", "ContentTypes", context);
    }

    public void PrepMigrations(Guid migrationId, string sourceFolder, MigrationContext context)
    {
        PrepContext(Path.Combine(sourceFolder, "DocumentType"), context);
    }
}
