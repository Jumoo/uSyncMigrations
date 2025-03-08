using uSync.BackOffice.Models;

namespace uSync.Migrations.Client;

public class uSyncMigrationAddOn : ISyncAddOn
{
    public string Name => uSyncMigrationsClient.AppName;

    public string Version => uSyncMigrationsClient.AppVersion;

    public string Icon => uSyncMigrationsClient.Icon;

    public string View => string.Empty; // $"{uSyncMigrations.PluginFolder}/dashboard.html";

    public string Alias => Name.ToLowerInvariant();

    public string DisplayName => Name;

    public int SortOrder => 19;
}
