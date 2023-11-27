using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;

using uSync.Migrations.Core.Services;

namespace uSync.Migrations.Core.Handlers.Seven;

[SyncMigrationHandler(BackOfficeConstants.Groups.Content, uSyncMigrations.Priorities.Content,
    SourceVersion = 7,
    SourceFolderName = "Content",
    TargetFolderName = "Content")]
internal class ContentMigrationHandler : ContentBaseMigrationHandler<Content>, ISyncMigrationHandler
{
    public ContentMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        IShortStringHelper shortStringHelper,
        ILogger<ContentMigrationHandler> logger)
        : base(eventAggregator, migrationFileService, shortStringHelper, logger)
    { }

    protected override string? GetEntityType()
    {
        return UmbConstants.UdiEntityType.Document;
    }
}
