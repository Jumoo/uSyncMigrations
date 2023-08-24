using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using uSync.Migrations.Configuration;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Eight;

[SyncMigrationHandler(BackOfficeConstants.Groups.Content, uSyncMigrations.Priorities.Content, 
    SourceVersion = 8,
    SourceFolderName = "Content",
    TargetFolderName = "Content")]
internal class ContentMigrationHandler : ContentBaseMigrationHandler<Content>, ISyncMigrationHandler
{
    public ContentMigrationHandler(
        IOptions<uSyncMigrationOptions> options,
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        IShortStringHelper shortStringHelper,
        ILogger<ContentMigrationHandler> logger)
        : base(options,eventAggregator, migrationFileService, shortStringHelper, logger)
    { }
}
