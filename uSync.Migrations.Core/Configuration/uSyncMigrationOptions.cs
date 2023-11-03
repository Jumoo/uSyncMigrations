namespace uSync.Migrations.Core.Configuration;

public class uSyncMigrationOptions
{
    public const string Section = "Usync:Migrations";
    public IList<string> DisabledHandlers { get; set; }
        = new List<string>();
}