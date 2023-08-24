using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using uSync.Migrations.Composing;
using uSync.Migrations.Configuration;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Seven;

[SyncMigrationHandler(BackOfficeConstants.Groups.Settings, uSyncMigrations.Priorities.MediaTypes, 
    SourceVersion = 7,
    SourceFolderName = "MediaType",
    TargetFolderName = "MediaTypes")]
internal class MediaTypeMigrationHandler : ContentTypeBaseMigrationHandler<MediaType>, ISyncMigrationHandler
{
    public MediaTypeMigrationHandler(
        IOptions<uSyncMigrationOptions> options,
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        ILogger<MediaTypeMigrationHandler> logger,
		IDataTypeService dataTypeService,
        IShortStringHelper shortStringHelper,
        Lazy<SyncMigrationHandlerCollection> migrationHandlers)
		: base(options,eventAggregator, migrationFileService, logger, dataTypeService, shortStringHelper, migrationHandlers)
	{ }
}
