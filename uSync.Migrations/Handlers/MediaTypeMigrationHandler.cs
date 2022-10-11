using System.Xml.Linq;

using Umbraco.Cms.Core.Events;

using uSync.Migrations.Models;
using uSync.Migrations.Notifications;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;

internal class MediaTypeMigrationHandler : ContentTypeBaseMigrationHandler, ISyncMigrationHandler
{
    public MediaTypeMigrationHandler(
        IEventAggregator eventAggregator,
        MigrationFileService migrationFileService,
        SyncMigratorCollection migrators) 
        : base(eventAggregator, migrationFileService, migrators, "MediaType")
    { }

    public int Priority => uSyncMigrations.Priorities.MediaTypes;

    public IEnumerable<MigrationMessage> MigrateFromDisk(Guid migrationId, string sourceFolder, MigrationContext context)
    {
        return base.MigrateFromDisk(
            migrationId, Path.Combine(sourceFolder, "MediaType"), "MediaType", "MediaTypes", context);
    }
    public void PrepMigrations(Guid migrationId, string sourceFolder, MigrationContext context)
    {
        PrepContext(Path.Combine(sourceFolder, "MediaType"), context);
    }

    protected override bool FireStartingNotification(XElement source)
    {
        var notification = new SyncMediaTypeMigratingNotification(source);
        _eventAggregator.PublishCancelable(notification);
        return !notification.Cancel;
    }

    protected override XElement FireCompletedNotification(XElement target)
    {
        // notification before we save anything.
        var notification = new SyncMediaTypeMigratedNotification(target);
        _eventAggregator.Publish(notification);
        return notification.Result;
    }

}
