using Umbraco.Cms.Core.Strings;

using uSync.Migrations.Migrators.Content;
using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;

internal class ContentMigrationHandler : ContentBaseMigrationHandler, ISyncMigrationHandler
{
    public ContentMigrationHandler(
        MigrationFileService migrationFileService,
        ContentPropertyMigrationCollection contentPropertyMigrators,
        IShortStringHelper shortStringHelper)
        : base(migrationFileService, contentPropertyMigrators, shortStringHelper)
    { }

    public int Priority => uSyncMigrations.Priorities.Content;

    public override string ItemType => "Content";

    public IEnumerable<MigrationMessage> MigrateFromDisk(Guid migrationId, string sourceFolder, MigrationContext context)
    {
        return DoMigrateFromDisk(migrationId, Path.Combine(sourceFolder, "Content"), context);
    }

    public void PrepMigrations(Guid migrationId, string sourceFolder, MigrationContext context)
    { }
}
