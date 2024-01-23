using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

using uSync.Migrations.Core.Handlers.Shared;
using uSync.Migrations.Core.Services;

namespace uSync.Migrations.Core.Handlers.Eight;

[SyncMigrationHandler(BackOfficeConstants.Groups.Settings, uSyncMigrations.Priorities.Dictionary,
    SourceVersion = 8,
    SourceFolderName = "Dictionary",
    TargetFolderName = "Dictionary")]
public class DictionaryMigrationHandler : SharedHandlerBase<DictionaryItem>, ISyncMigrationHandler
{
    public DictionaryMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        ILogger<DictionaryMigrationHandler> logger)
        : base(eventAggregator, migrationFileService, logger)
    {
    }
}