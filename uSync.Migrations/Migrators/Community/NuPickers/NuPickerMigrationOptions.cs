namespace uSync.Migrations.Migrators.Community;

public class NuPickerMigrationOptions
{
    public const string Section = "Usync:Migrations:NuPickers";
    public IDictionary<string,string>? AssembliesMapping { get; set; }
    public IDictionary<string,string>? NamespacesMapping { get; set; }
}