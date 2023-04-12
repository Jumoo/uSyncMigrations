using uSync.Migrations.Context;

namespace uSync.Migrations.Migrators.Community.Archetype;

public class DefaultArchetypeMigrationConfigurer : IArchetypeMigrationConfigurer
{
    public string GetBlockElementAlias(string archetypeAlias, SyncMigrationContext context)
    {
        return archetypeAlias;
    }
}
