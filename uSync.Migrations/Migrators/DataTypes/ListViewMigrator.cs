using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.DataTypes;
internal class ListViewMigrator : DataTypeMigratorBase
{
    public override string[] Editors => new[] { "Umbraco.ListView" };

    public override object GetConfigValues(SyncDataTypeInfo dataTypeInfo)
        => dataTypeInfo.ConvertPreValuesToJson(true);
}
