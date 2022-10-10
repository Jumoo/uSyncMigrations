using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

/// <summary>
///  Migrators to be used on DataTypes
/// </summary>
public interface ISyncDataTypeMigrator : ISyncItemMigrator
{
    public string GetDataType(SyncDataTypeInfo dataTypeInfo);

    public string GetDatabaseType(SyncDataTypeInfo dataTypeInfo);

    public object GetConfigValues(SyncDataTypeInfo dataTypeInfo);
}
