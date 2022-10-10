namespace uSync.Migrations.Migrators;
public interface ISyncItemMigrator
{
    /// <summary>
    ///  editors this migration is good for. 
    /// </summary>
    string[] Editors { get; }
}
