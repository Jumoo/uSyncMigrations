namespace uSync.Migrations.Handlers;
internal class SyncMigrationHandlerAttribute : Attribute
{
    public SyncMigrationHandlerAttribute(string group, int priority, int sourceVersion)
    {
        Group = group;
        Priority = priority;
        SourceVersion = sourceVersion;
    }

    public string Group { get; set; }
    public int Priority { get; set; }
    public int SourceVersion { get; set; }

    public string? SourceFolderName { get; set; }
    public string? TargetFolderName { get; set; }
}
