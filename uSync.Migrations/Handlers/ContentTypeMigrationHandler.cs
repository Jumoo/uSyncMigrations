using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

using uSync.Migrations.Composing;
using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;

internal class ContentTypeMigrationHandler : ContentTypeBaseMigrationHandler<ContentType>, ISyncMigrationHandler
{
    private IFileService _fileService;

    public ContentTypeMigrationHandler(
        IEventAggregator eventAggregator,
        SyncMigrationFileService migrationFileService,
        SyncPropertyMigratorCollection migrators,
        IFileService fileService)
        : base(eventAggregator, migrationFileService, migrators)
    {
        _fileService = fileService;
    }

    public string ItemType => nameof(ContentType);

    public int Priority => uSyncMigrations.Priorities.ContentTypes;

    public void PrepareMigrations(Guid migrationId, string sourceFolder, SyncMigrationContext context)
    {
        PrepareContext(Path.Combine(sourceFolder, "DocumentType"), context);

        foreach (var template in _fileService.GetTemplates())
        {
            context.AddTemplateKey(template.Alias, template.Key);
        }
    }

    public IEnumerable<MigrationMessage> MigrateFromDisk(Guid migrationId, string sourceFolder, SyncMigrationContext context)
        => DoMigrateFromDisk(migrationId, Path.Combine(sourceFolder, "DocumentType"), ItemType, "ContentTypes", context);
}
