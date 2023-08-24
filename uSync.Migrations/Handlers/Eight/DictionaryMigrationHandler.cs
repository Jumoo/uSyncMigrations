using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using uSync.Migrations.Configuration;
using uSync.Migrations.Handlers.Shared;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Eight;

[SyncMigrationHandler(BackOfficeConstants.Groups.Settings, uSyncMigrations.Priorities.Dictionary, 
    SourceVersion = 8,
    SourceFolderName = "Dictionary",
    TargetFolderName = "Dictionary")]
internal class DictionaryMigrationHandler : SharedHandlerBase<DictionaryItem>, ISyncMigrationHandler
{
    public DictionaryMigrationHandler(
        IOptions<uSyncMigrationOptions> options,
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        ILogger<DictionaryMigrationHandler> logger)
        : base(options,eventAggregator, migrationFileService, logger)
    {
    }
}