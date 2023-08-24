using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using uSync.Migrations.Configuration;
using uSync.Migrations.Handlers.Shared;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Eight;

[SyncMigrationHandler(BackOfficeConstants.Groups.Settings, uSyncMigrations.Priorities.Languages, 
    SourceVersion = 8,
    SourceFolderName = "Languages",
    TargetFolderName = "Languages")]
internal class LanguageMigrationHandler : SharedHandlerBase<Language>, ISyncMigrationHandler
{
    public LanguageMigrationHandler(
        IOptions<uSyncMigrationOptions> options,
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        ILogger<LanguageMigrationHandler> logger) 
        : base(options,eventAggregator, migrationFileService, logger)
    { }
}


