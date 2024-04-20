global using UmbConstants = Umbraco.Cms.Core.Constants;
global using BackOfficeConstants = uSync.BackOffice.uSyncConstants;

namespace uSync.Migrations.Core;

public static class uSyncMigrations
{
    public const string SourceFolder = "uSync/data";

    public const string MigrationFolder = "uSync/Migrations";

    public static class Priorities
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

    public static class EditorAliases
    {
        /// <remarks>
        ///  at some point this will leave the core code base, so we need to start 
        ///  refencing our own versions of some alias values.
        /// </remarks>
        public const string NestedContent = "Umbraco.NestedContent";
        public const string NestedContentCommunity = "Our.Umbraco.NestedContent";

        public const string Grid = "Umbraco.Grid";


	}
}


public static class uSyncMigrationsAuthorizationPolicies
{
    public const string MigrationsTreeAccess = nameof(MigrationsTreeAccess);
}