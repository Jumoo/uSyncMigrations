using Lucene.Net.Documents;

namespace uSync.Migrations.Migrators;

/// <summary>
///  migrator to be used on Content properties 
/// </summary>
public interface ISyncContentPropertyMigrator : ISyncItemMigrator
{
    public string GetMigratedValue(string editorAlias, string value);
}