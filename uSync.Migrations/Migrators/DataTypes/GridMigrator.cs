using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.DataTypes;

internal class GridMigrator : DataTypeMigratorBase
{
    public override string[] Editors => new[] { "Umbraco.Grid" };

    public override object GetConfigValues(SyncDataTypeInfo dataTypeInfo)
        => dataTypeInfo.MapPreValues(new GridConfiguration());
}
