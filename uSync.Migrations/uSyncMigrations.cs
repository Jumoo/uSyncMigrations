global using UmbConstants = Umbraco.Cms.Core.Constants;
global using BackOfficeConstants = uSync.BackOffice.uSyncConstants;

using Org.BouncyCastle.Bcpg.Sig;

namespace uSync.Migrations;

public static class uSyncMigrations
{
    public const string AppName = "Migrations";

    public const string TreeName = "uSyncMigrations";

    public const string Icon = "icon-filter-arrows";
    public static string AppVersion => typeof(uSyncMigrations).Assembly.GetName().Version?.ToString(3) ?? "10.0.0";

    public const string PluginFolder = "/App_Plugins/uSyncMigrations";

    public const string SourceFolder = "uSync/data";

    public const string MigrationFolder = "uSync/Migrations";

    internal static class Priorities
    {
        public const int Templates = 5;

        public const int Languages = 6;

        public const int Dictionary = 7;

        public const int DataTypes = 10;

        public const int ContentTypes = 20;

        public const int MediaTypes = 30;

        public const int Macros = 40;

        public const int Content = 100;

        public const int Media = 110;
    }
}


public static class uSyncMigrationsAuthorizationPolicies
{
    public const string MigrationsTreeAccess = nameof(MigrationsTreeAccess);
}