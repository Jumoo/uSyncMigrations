﻿using uSync.Migrations.Core;
using uSync.Migrations.Core.Handlers;
using uSync.Migrations.Core.Plans;
using uSync.Migrations.Core.Plans.Models;

namespace uSync.Migrations.Client.Plans;

public class FullSevenToElevenAllTheThingsPlan : ISyncMigrationPlan
{
    private readonly SyncMigrationHandlerCollection _migrationHandlers;

    public FullSevenToElevenAllTheThingsPlan(SyncMigrationHandlerCollection migrationHandlers)
    {
        _migrationHandlers = migrationHandlers;
    }

    public int Order => 300;

    public string Name => "One step upgrade (blocklist, block grid)";

    public string Icon => "icon-bang";

    public string Description => "Does everything at once, upgrades all the things and converts" +
        "nested content to BlockList and Grids to BlockGrids";

    public MigrationOptions Options => new MigrationOptions
    {
        Target = $"{uSyncMigrations.MigrationFolder}/{DateTime.Now:yyyyMMdd_HHmmss}",
        Handlers = _migrationHandlers.SelectGroup(7, string.Empty),
        SourceVersion = 7,
        PreferredMigrators = new Dictionary<string, string>
        {
            { uSyncMigrations.EditorAliases.NestedContent, "NestedToBlockListMigrator" },
            { uSyncMigrations.EditorAliases.Grid, "GridToBlockGridMigrator" }
        }
    };
}
