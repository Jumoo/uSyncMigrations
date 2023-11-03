using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;

using uSync.Migrations.Core.Handlers.Shared;
using uSync.Migrations.Core.Services;

namespace uSync.Migrations.Core.Handlers.Eight;

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
