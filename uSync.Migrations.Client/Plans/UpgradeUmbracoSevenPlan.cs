using uSync.Migrations.Core;
using uSync.Migrations.Core.Handlers;
using uSync.Migrations.Core.Plans;
using uSync.Migrations.Core.Plans.Models;

namespace uSync.Migrations.Client.Plans;

public class UpgradeUmbracoSevenPlan : ISyncMigrationPlan
{
    public int Order => 120;

    private readonly SyncMigrationHandlerCollection _migrationHandlers;

    public UpgradeUmbracoSevenPlan(
        SyncMigrationHandlerCollection migrationHandlers)
    {
        _migrationHandlers = migrationHandlers;
    }

    public string Name => "Upgrade Plan";

    public string Icon => "icon-paper-plane color-orange";

    public string Description => "Upgrade everything (don't convert)";

    public MigrationOptions Options => new MigrationOptions
    {
        Target = $"{uSyncMigrations.MigrationFolder}/{DateTime.Now:yyyyMMdd_HHmmss}",
        Handlers = _migrationHandlers.SelectGroup(7, string.Empty)
    };
}