using uSync.BackOffice.Models;

namespace uSync.Migrations.Lite;
public class uSyncMigrationsLiteAddOn : ISyncAddOn
{
    public string Name => uSyncMigrationsLite.Name;

    public string Version => uSyncMigrationsLite.AppVersion;

    public string Icon => uSyncMigrationsLite.Icon;

    public string View => string.Empty;

    public string Alias => Name.ToLowerInvariant();

    public string DisplayName => Name;

    public int SortOrder => 19;
}
