using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;

using uSync.Core;
using uSync.Migrations;
using uSync.Migrations.Migrators;
using uSync.Migrations.Notifications;

namespace MyMigrations;

/// <summary>
///  this is an example of how you might extend, and replace
///  core migrations for certain types (e.g the grid)
/// </summary>
/// <remarks>
///  by default the core will convert what it can, but if you
///  want to do something special, you can implement your 
///  own ISyncItemMigrations and remove the core ones .
/// </remarks>


/// **********************************************************
/// NOTE: This project is not referenced in the Sample site
///       so this code isn't going to actually fire
/// **********************************************************

[ComposeAfter(typeof(SyncMigrationComposer))]
public class MyMigrationComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // if we have a custom migration for the grid, we should 
        // remove the existing one. 
        builder.Migrators().Remove<GridMigrator>();

        // our migration will be discovered, if it implements ISyncItemMigrator

        // you can also intercept with notifications
        builder.AddNotificationHandler<SyncContentTypeMigratedNotification,
            ContentTypeMigratedNotificationHandler>();
    }
}

/// <summary>
///  the migrated handler is fired pre save to disk, so 
///  if you want to do extra work on the xml you could do it here. 
/// </summary>
public class ContentTypeMigratedNotificationHandler
    : INotificationHandler<SyncContentTypeMigratedNotification>
{
    public void Handle(SyncContentTypeMigratedNotification notification)
    {
        // you can also use the usync xml helpers, to quickly get 
        // things like the alias or the key. 
        var alias = notification.Result.GetAlias();
        var key = notification.Result.GetKey();
       
    }
}
