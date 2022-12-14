using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

using uSync.Migrations.Handlers.Shared;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Eight;

[SyncMigrationHandler(BackOfficeConstants.Groups.Settings, uSyncMigrations.Priorities.Macros,
    SourceVersion = 8,
    SourceFolderName = "Macros",
    TargetFolderName = "Macros")]
internal class MacroMigrationHandler : SharedHandlerBase<Macro>, ISyncMigrationHandler
{
    public MacroMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        ILogger<MacroMigrationHandler> logger) 
        : base(eventAggregator, migrationFileService, logger)
    { }
}
