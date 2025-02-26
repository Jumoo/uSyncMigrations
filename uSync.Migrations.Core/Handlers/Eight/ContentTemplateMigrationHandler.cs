using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using uSync.Migrations.Core.Services;

namespace uSync.Migrations.Core.Handlers.Eight;

[SyncMigrationHandler(BackOfficeConstants.Groups.Content, uSyncMigrations.Priorities.ContentTemplate,
    SourceVersion = 8,
    SourceFolderName = "Blueprints",
    TargetFolderName = "Blueprints")]
internal class ContentTemplateMigrationHandler : ContentBaseMigrationHandler<Content>, ISyncMigrationHandler
{
    public ContentTemplateMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        IShortStringHelper shortStringHelper,
        ILogger<ContentMigrationHandler> logger)
        : base(eventAggregator, migrationFileService, shortStringHelper, logger)
    { }

    protected override string? GetEntityType()
    {
        return UmbConstants.UdiEntityType.DocumentBlueprint;
    }
}