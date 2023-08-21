namespace uSync.Migrations.Migrators.Community.Archetype;

public class ArchetypeMigrationOptions
{
    public const string Section = "Usync:Migrations:Archetype";
    public List<string>? NotMergableDocumentTypes { get; set; }
}