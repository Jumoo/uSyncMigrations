using Archetype.Models;
using uSync.Migrations.Context;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators.Community.Archetype;

public interface IArchetypeMigrationConfigurer
{
    public string GetBlockElementAlias(ArchetypeFieldsetModel archetypeAlias,
        SyncMigrationContentProperty dataTypeProperty, SyncMigrationContext context);

    public string GetBlockElementAlias(ArchetypePreValueFieldset archetypeAlias, SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context);
}
