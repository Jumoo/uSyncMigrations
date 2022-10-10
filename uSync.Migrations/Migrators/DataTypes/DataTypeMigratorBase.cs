using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.DataTypes;
public abstract class DataTypeMigratorBase : ISyncDataTypeMigrator
{
    public abstract string[] Editors { get; }

    public abstract object GetConfigValues(SyncDataTypeInfo dataTypeInfo);

    public virtual string GetDatabaseType(SyncDataTypeInfo dataTypeInfo)
        => dataTypeInfo.DatabaseType;

    public virtual string GetDataType(SyncDataTypeInfo dataTypeInfo)
        => dataTypeInfo.EditorAlias;
}
