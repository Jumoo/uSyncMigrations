using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

using uSync.Migrations.Composing;
using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;

internal class MediaTypeMigrationHandler : ContentTypeBaseMigrationHandler<MediaType>, ISyncMigrationHandler
{
    public MediaTypeMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        SyncPropertyMigratorCollection migrators)
        : base(eventAggregator, migrationFileService, migrators)
    { }

    public string Group => uSync.BackOffice.uSyncConstants.Groups.Settings;

    public string ItemType => nameof(MediaType);

    public int Priority => uSyncMigrations.Priorities.MediaTypes;

    public void PrepareMigrations(Guid migrationId, string sourceFolder, SyncMigrationContext context)
        => PrepareContext(Path.Combine(sourceFolder, nameof(MediaType)), context);

    public IEnumerable<MigrationMessage> MigrateFromDisk(Guid migrationId, string sourceFolder, SyncMigrationContext context)
        => DoMigrateFromDisk(migrationId, Path.Combine(sourceFolder, ItemType), ItemType, "MediaTypes", context);
}
