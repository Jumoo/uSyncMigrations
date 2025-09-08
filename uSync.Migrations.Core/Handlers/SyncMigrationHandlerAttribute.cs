namespace uSync.Migrations.Core.Handlers;

public class SyncMigrationHandlerAttribute : Attribute
{
    public SyncMigrationHandlerAttribute(string group, int priority)
    {
        Group = group;
        Priority = priority;
    }

    public string Group { get; set; }
    public int Priority { get; set; }
    public int SourceVersion { get; set; }

    public string? SourceFolderName { get; set; }
    public string? TargetFolderName { get; set; }
}
