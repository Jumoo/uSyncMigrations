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

[SyncMigrationHandler(BackOfficeConstants.Groups.Settings, uSyncMigrations.Priorities.ContentTypes,
    SourceVersion = 7,
    SourceFolderName = "DocumentType",
    TargetFolderName = "ContentTypes")]
internal class ContentTypeMigrationHandler : ContentTypeBaseMigrationHandler<ContentType>, ISyncMigrationHandler
{
    public ContentTypeMigrationHandler(
        IOptions<uSyncMigrationOptions> options,
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        ILogger<ContentTypeMigrationHandler> logger,
		IDataTypeService dataTypeService,
        IShortStringHelper shortStringHelper, 
        Lazy<SyncMigrationHandlerCollection> migrationHandlers)
		: base(options,eventAggregator, migrationFileService, logger, dataTypeService, shortStringHelper, migrationHandlers)
	{ }
}
