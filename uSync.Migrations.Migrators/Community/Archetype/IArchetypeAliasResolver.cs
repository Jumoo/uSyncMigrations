using uSync.Migrations.Migrators.Community.Archetype.Models;

namespace uSync.Migrations.Migrators.Community.Archetype;

public interface IArchetypeAliasResolver
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="fieldSetAlias"></param>
    /// <param name="dataTypeAlias"></param>
    /// <returns></returns>
    string GetBlockElementAlias(string fieldSetAlias, string dataTypeAlias);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="contentProperty"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    string GetDataTypeAlias(SyncMigrationContentProperty contentProperty, SyncMigrationContext context);
}