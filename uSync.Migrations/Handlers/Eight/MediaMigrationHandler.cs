using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using uSync.Migrations.Configuration;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Eight;

[SyncMigrationHandler(BackOfficeConstants.Groups.Content, uSyncMigrations.Priorities.Media, 
    SourceVersion = 8,
    SourceFolderName = "Media",
    TargetFolderName = "Media")]
internal class MediaMigrationHandler : ContentBaseMigrationHandler<Media>, ISyncMigrationHandler
{
    public MediaMigrationHandler(
        IOptions<uSyncMigrationOptions> options,
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        IShortStringHelper shortStringHelper,
        ILogger<MediaMigrationHandler> logger) 
        : base(options,eventAggregator, migrationFileService, shortStringHelper, logger)
    { }
}