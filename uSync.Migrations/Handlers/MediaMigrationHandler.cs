using Umbraco.Cms.Core.Strings;

using uSync.Migrations.Migrators.Content;
using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;

internal class MediaMigrationHandler : ContentBaseMigrationHandler, ISyncMigrationHandler
{
    public int Priority => uSyncMigrations.Priorities.Media;

    public override string ItemType => "Media";

    public MediaMigrationHandler(
        MigrationFileService migrationFileService,
        ContentPropertyMigrationCollection contentPropertyMigrators,
        IShortStringHelper shortStringHelper)
        : base(migrationFileService, contentPropertyMigrators, shortStringHelper)
    { }

    public IEnumerable<MigrationMessage> MigrateFromDisk(Guid migrationId, string sourceFolder, MigrationContext context)
    {
        return DoMigrateFromDisk(migrationId, Path.Combine(sourceFolder, "Media"), context);
    }

    public void PrepMigrations(Guid migrationId, string sourceFolder, MigrationContext context)
    { }
}
