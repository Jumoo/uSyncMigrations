﻿using uSync.Migrations.Core.Configuration.Models;
using uSync.Migrations.Core.Handlers;

namespace uSync.Migrations.Core.Extensions;
public static class SyncMigrationHandlersExtensions
{
    public static HandlerOption ToHandlerOption(this ISyncMigrationHandler handler, bool include)
        => new HandlerOption { Name = handler.ItemType, Include = include };
}
