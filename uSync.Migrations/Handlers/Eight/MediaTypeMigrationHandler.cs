using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using uSync.Migrations.Composing;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Eight;

[SyncMigrationHandler(BackOfficeConstants.Groups.Settings, uSyncMigrations.Priorities.MediaTypes, 
    SourceVersion = 8,
    SourceFolderName = "MediaTypes",
    TargetFolderName = "MediaTypes")]
internal class MediaTypeMigrationHandler : ContentTypeBaseMigrationHandler<MediaType>, ISyncMigrationHandler
{
    public MediaTypeMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        ILogger<MediaTypeMigrationHandler> logger,
		IDataTypeService dataTypeService, 
        Lazy<SyncMigrationHandlerCollection> migrationHandlers)
		: base(eventAggregator, migrationFileService, logger, dataTypeService, migrationHandlers)
	{
    }
}
