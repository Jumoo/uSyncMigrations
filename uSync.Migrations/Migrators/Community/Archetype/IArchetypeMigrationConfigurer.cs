using uSync.Migrations.Context;

namespace uSync.Migrations.Migrators.Community.Archetype;

public interface IArchetypeMigrationConfigurer
{
    public string GetBlockElementAlias(string archetypeAlias, SyncMigrationContext context);
}
