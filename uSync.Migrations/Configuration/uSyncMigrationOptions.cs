namespace uSync.Migrations.Configuration;

public class uSyncMigrationOptions
{
    public const string Section = "Usync:Migrations";
    public IList<string>? DisabledHandlers { get; set; }
    //todo might be worth split into types
    public IDictionary<string,IDictionary<string,string>>? OverrideAliases { get; set; }
}