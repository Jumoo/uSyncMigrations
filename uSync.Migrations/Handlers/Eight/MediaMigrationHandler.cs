using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;

using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Eight;

[SyncMigrationHandler(BackOfficeConstants.Groups.Content, uSyncMigrations.Priorities.Media, 
    SourceVersion = 8,
    SourceFolderName = "Media",
    TargetFolderName = "Media")]
internal class MediaMigrationHandler : ContentBaseMigrationHandler<Media>, ISyncMigrationHandler
{
    public MediaMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        IShortStringHelper shortStringHelper,
        ILogger<MediaMigrationHandler> logger) 
        : base(eventAggregator, migrationFileService, shortStringHelper, logger)
    { }
}