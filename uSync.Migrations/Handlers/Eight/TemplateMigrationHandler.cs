using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;

using uSync.Migrations.Handlers.Shared;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Eight;

[SyncMigrationHandler(BackOfficeConstants.Groups.Settings, uSyncMigrations.Priorities.Templates,
    SourceVersion = 8,
    SourceFolderName = "Templates",
    TargetFolderName = "Templates")]
internal class TemplateMigrationHandler : SharedTemplateHandler, ISyncMigrationHandler
{
    public TemplateMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        IFileService fileService,
        ILogger<TemplateMigrationHandler> logger) 
        : base(eventAggregator, migrationFileService, fileService, logger)
    { } 
}
