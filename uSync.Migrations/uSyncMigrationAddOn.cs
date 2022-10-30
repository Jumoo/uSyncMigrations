using uSync.BackOffice.Models;

namespace uSync.Migrations;

public class uSyncMigrationAddOn : ISyncAddOn
{
    public string Name => uSyncMigrations.AppName;

    public string Version => uSyncMigrations.AppVersion;

    public string Icon => "icon-filter-arrows";

    public string View => $"{uSyncMigrations.PluginFolder}/dashboard.html";

    public string Alias => Name.ToLowerInvariant();

    public string DisplayName => Name;

    public int SortOrder => 19;
}
