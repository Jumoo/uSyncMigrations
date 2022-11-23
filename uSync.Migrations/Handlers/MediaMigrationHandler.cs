using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;

using uSync.Migrations.Composing;
using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;

internal class MediaMigrationHandler : ContentBaseMigrationHandler<Media>, ISyncMigrationHandler
{
    public MediaMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        IShortStringHelper shortStringHelper)
        : base(eventAggregator, migrationFileService, shortStringHelper)
    {
        _ignoredProperties.UnionWith(new[]
        {
            UmbConstants.Conventions.Media.Bytes,
            UmbConstants.Conventions.Media.Extension,
            UmbConstants.Conventions.Media.Height,
            UmbConstants.Conventions.Media.Width,
        });
    }

    public string ItemType => nameof(Media);

    public int Priority => uSyncMigrations.Priorities.Media;

    public void PrepareMigrations(Guid migrationId, string sourceFolder, SyncMigrationContext context)
    { }

    public IEnumerable<MigrationMessage> MigrateFromDisk(Guid migrationId, string sourceFolder, SyncMigrationContext context)
        => DoMigrateFromDisk(migrationId, Path.Combine(sourceFolder, nameof(Media)), context);
}
