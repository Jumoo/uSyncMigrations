namespace uSync.Migrations.Lite;
internal static class uSyncMigrationsLite
{
    public const string Name = "Migrations";
    public const string Icon = "icon-filter-arrows";

    public static string AppVersion => typeof(uSyncMigrationsLite).Assembly.GetName().Version?.ToString(3) ?? "10.0.0";

}
