﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using uSync.Migrations.Configuration;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Seven;

[SyncMigrationHandler(BackOfficeConstants.Groups.Content, uSyncMigrations.Priorities.Media, 
    SourceVersion = 7,
    SourceFolderName = "Media",
    TargetFolderName = "Media")]
internal class MediaMigrationHandler : ContentBaseMigrationHandler<Media>, ISyncMigrationHandler
{
    public MediaMigrationHandler(
        IOptions<uSyncMigrationOptions> options,
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        IShortStringHelper shortStringHelper,
        ILogger<MediaMigrationHandler> logger)
        : base(options,eventAggregator, migrationFileService, shortStringHelper, logger)
    {
        _ignoredProperties.UnionWith(new[]
        {
            UmbConstants.Conventions.Media.Bytes,
            UmbConstants.Conventions.Media.Extension,
            UmbConstants.Conventions.Media.Height,
            UmbConstants.Conventions.Media.Width,
        });

        _mediaTypeAliasForFileExtension.Union(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "docx", UmbConstants.Conventions.MediaTypes.ArticleAlias },
            { "doc", UmbConstants.Conventions.MediaTypes.ArticleAlias },
            { "pdf", UmbConstants.Conventions.MediaTypes.ArticleAlias },
            { "mp3", UmbConstants.Conventions.MediaTypes.AudioAlias },
            { "weba", UmbConstants.Conventions.MediaTypes.AudioAlias },
            { "oga", UmbConstants.Conventions.MediaTypes.AudioAlias },
            { "opus", UmbConstants.Conventions.MediaTypes.AudioAlias },
            { "svg", UmbConstants.Conventions.MediaTypes.VectorGraphicsAlias },
            { "mp4", UmbConstants.Conventions.MediaTypes.VideoAlias },
            { "ogv", UmbConstants.Conventions.MediaTypes.VideoAlias },
            { "webm", UmbConstants.Conventions.MediaTypes.VideoAlias },
        });
    } 
}
