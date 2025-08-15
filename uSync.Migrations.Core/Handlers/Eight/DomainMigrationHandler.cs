using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using uSync.Migrations.Core.Handlers.Shared;
using uSync.Migrations.Core.Services;

namespace uSync.Migrations.Core.Handlers.Eight;

[SyncMigrationHandler(BackOfficeConstants.Groups.Settings, uSyncMigrations.Priorities.Domains,
    SourceVersion = 8,
    SourceFolderName = "Domains", TargetFolderName = "Domains")]
internal class DomainMigrationHandler : SharedHandlerBase<IDomain>, ISyncMigrationHandler
{
    public DomainMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        ILogger<DomainMigrationHandler> logger)
        : base(eventAggregator, migrationFileService, logger)
    {
    }
    
    // For v8 to v13, domains generally pass through unchanged
    // The base SharedHandlerBase implementation handles this correctly
}