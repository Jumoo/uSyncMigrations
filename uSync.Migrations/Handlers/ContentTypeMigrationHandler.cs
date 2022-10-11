using System.Xml.Linq;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.DependencyInjection;

using uSync.Migrations.Models;
using uSync.Migrations.Notifications;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;

internal class ContentTypeMigrationHandler : ContentTypeBaseMigrationHandler, ISyncMigrationHandler
{
    private IFileService _fileService;

    public ContentTypeMigrationHandler(
        IEventAggregator eventAggregator,
        MigrationFileService migrationFileService,
        SyncMigratorCollection migrators,
        IFileService fileService)
        : base(eventAggregator, migrationFileService, migrators, "ContentType")
    {
        _fileService = fileService;
    }

    public int Priority => 20;

    public IEnumerable<MigrationMessage> MigrateFromDisk(Guid migrationId, string sourceFolder, MigrationContext context)
    {
        return base.MigrateFromDisk(
            migrationId, Path.Combine(sourceFolder, "DocumentType"), "ContentType", "ContentTypes", context);
    }

    public void PrepMigrations(Guid migrationId, string sourceFolder, MigrationContext context)
    {
        PrepContext(Path.Combine(sourceFolder, "DocumentType"), context);

        foreach (var template in _fileService.GetTemplates())
        {
            context.AddTemplateKey(template.Alias, template.Key);
        }
    }

    protected override bool FireStartingNotification(XElement source)
    {
        var notification = new SyncContentTypeMigratingNotification(source);
        _eventAggregator.PublishCancelable(notification);
        return !notification.Cancel;
    }

    protected override XElement FireCompletedNotification(XElement target)
    {
        // notification before we save anything.
        var notification = new SyncContentTypeMigratedNotification(target);
        _eventAggregator.Publish(notification);
        return notification.Result;
    }
}
