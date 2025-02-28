global using UmbConstants = Umbraco.Cms.Core.Constants;
global using BackOfficeConstants = uSync.BackOffice.uSyncConstants;

public static class uSyncMigrationsClient
{
    public const string AppName = "uSync Migrations";

    public const string TreeName = "uSyncMigrations";

    public const string Icon = "icon-filter-arrows";
    public static string AppVersion => typeof(uSyncMigrationsClient).Assembly.GetName().Version?.ToString(3) ?? "10.0.0";

    public const string PluginFolder = "/App_Plugins/uSyncMigrations";

}