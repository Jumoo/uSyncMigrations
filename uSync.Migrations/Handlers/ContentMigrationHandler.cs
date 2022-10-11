using Umbraco.Cms.Core.Strings;

using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;

internal class ContentMigrationHandler : ContentBaseMigrationHandler, ISyncMigrationHandler
{
    public ContentMigrationHandler(
        MigrationFileService migrationFileService,
        SyncMigratorCollection migrators,
        IShortStringHelper shortStringHelper)
        : base(migrationFileService, migrators, shortStringHelper, "Content")
    { }

    public int Priority => uSyncMigrations.Priorities.Content;

    public IEnumerable<MigrationMessage> MigrateFromDisk(Guid migrationId, string sourceFolder, MigrationContext context)
        => DoMigrateFromDisk(migrationId, Path.Combine(sourceFolder, "Content"), context);

    public void PrepMigrations(Guid migrationId, string sourceFolder, MigrationContext context)
    { }
}
