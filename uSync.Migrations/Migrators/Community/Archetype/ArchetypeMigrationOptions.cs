namespace uSync.Migrations.Migrators.Community.Archetype;

public class ArchetypeMigrationOptions
{
    public const string Section = "Usync:Migrations:Archetype";
    public List<string>? NotMergableDocumentTypes { get; set; } = new List<string>();
    public Dictionary<string,string>? RenamedDocumentTypes{ get; set; } = new Dictionary<string, string>();

}