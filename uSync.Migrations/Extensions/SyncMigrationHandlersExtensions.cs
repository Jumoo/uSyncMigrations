using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Handlers;

namespace uSync.Migrations.Extensions;
public static class SyncMigrationHandlersExtensions
{
    public static HandlerOption ToHandlerOption(this ISyncMigrationHandler handler, bool include)
        => new HandlerOption { Name = handler.ItemType, Include = include };
}
