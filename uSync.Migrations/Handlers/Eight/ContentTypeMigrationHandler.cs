using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Eight;

[SyncMigrationHandler(BackOfficeConstants.Groups.Settings, uSyncMigrations.Priorities.ContentTypes, 
    SourceVersion = 8,
    SourceFolderName = "ContentTypes",
    TargetFolderName = "ContentTypes")]
internal class ContentTypeMigrationHandler : ContentTypeBaseMigrationHandler<ContentType>, ISyncMigrationHandler
{
    public ContentTypeMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        ILogger<ContentTypeMigrationHandler> logger) 
        : base(eventAggregator, migrationFileService, logger)
    { }
}
