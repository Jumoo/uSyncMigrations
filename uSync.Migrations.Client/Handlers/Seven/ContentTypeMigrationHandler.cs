using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

using uSync.Migrations.Core;
using uSync.Migrations.Core.Handlers;
using uSync.Migrations.Core.Services;

namespace uSync.Migrations.Client.Handlers.Seven;

[SyncMigrationHandler(BackOfficeConstants.Groups.Settings, uSyncMigrations.Priorities.ContentTypes,
    SourceVersion = 7,
    SourceFolderName = "DocumentType",
    TargetFolderName = "ContentTypes")]
public class ContentTypeMigrationHandler : ContentTypeBaseMigrationHandler<ContentType>, ISyncMigrationHandler
{
    public ContentTypeMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        ILogger<ContentTypeMigrationHandler> logger,
        IDataTypeService dataTypeService,
        IShortStringHelper shortStringHelper,
        Lazy<SyncMigrationHandlerCollection> migrationHandlers)
        : base(eventAggregator, migrationFileService, logger, dataTypeService, shortStringHelper, migrationHandlers)
    { }
}
