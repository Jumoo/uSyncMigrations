using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Eight;

[SyncMigrationHandler(BackOfficeConstants.Groups.Content, uSyncMigrations.Priorities.Content, 
    SourceVersion = 8,
    SourceFolderName = "Content",
    TargetFolderName = "Content")]
internal class ContentMigrationHandler : ContentBaseMigrationHandler<Content>, ISyncMigrationHandler
{
    public ContentMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        IShortStringHelper shortStringHelper,
        IContentTypeService contentTypeService,
        IDataTypeService dataTypeService,
        ILogger<ContentMigrationHandler> logger)
        : base(eventAggregator, migrationFileService, shortStringHelper, contentTypeService, dataTypeService, logger)
    { }
}
