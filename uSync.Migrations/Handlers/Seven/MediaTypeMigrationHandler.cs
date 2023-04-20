using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using uSync.Migrations.Composing;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Seven;

[SyncMigrationHandler(BackOfficeConstants.Groups.Settings, uSyncMigrations.Priorities.MediaTypes, 
    SourceVersion = 7,
    SourceFolderName = "MediaType",
    TargetFolderName = "MediaTypes")]
internal class MediaTypeMigrationHandler : ContentTypeBaseMigrationHandler<MediaType>, ISyncMigrationHandler
{
    public MediaTypeMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        ILogger<MediaTypeMigrationHandler> logger,
		IDataTypeService dataTypeService,
        IShortStringHelper shortStringHelper,
        Lazy<SyncMigrationHandlerCollection> migrationHandlers)
		: base(eventAggregator, migrationFileService, logger, dataTypeService, shortStringHelper, migrationHandlers)
	{ }
}
