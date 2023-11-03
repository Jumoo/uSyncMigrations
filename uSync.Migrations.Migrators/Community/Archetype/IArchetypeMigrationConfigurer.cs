using uSync.Migrations.Migrators.Community.Archetype.Models;

namespace uSync.Migrations.Migrators.Community.Archetype;

public interface IArchetypeMigrationConfigurer
{
    public string GetBlockElementAlias(ArchetypeFieldsetModel archetypeAlias,
        SyncMigrationContentProperty dataTypeProperty, SyncMigrationContext context);

    public string GetBlockElementAlias(ArchetypePreValueFieldSet archetypeAlias, SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context);
}