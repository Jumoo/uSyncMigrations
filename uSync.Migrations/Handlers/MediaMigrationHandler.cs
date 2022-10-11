using Umbraco.Cms.Core.Strings;

using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;

internal class MediaMigrationHandler : ContentBaseMigrationHandler, ISyncMigrationHandler
{
    public int Priority => uSyncMigrations.Priorities.Media;

    public MediaMigrationHandler(
        MigrationFileService migrationFileService,
        SyncMigratorCollection migrators,
        IShortStringHelper shortStringHelper)
        : base(migrationFileService, migrators, shortStringHelper, "Media")
    { }

    public IEnumerable<MigrationMessage> MigrateFromDisk(Guid migrationId, string sourceFolder, MigrationContext context)
        => DoMigrateFromDisk(migrationId, Path.Combine(sourceFolder, "Media"), context);

    public void PrepMigrations(Guid migrationId, string sourceFolder, MigrationContext context)
    { }
}
