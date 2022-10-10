using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Extensions;
using uSync.Migrations.Models;


namespace uSync.Migrations.Migrators.DataTypes;
internal class DatePickerMigrator : DataTypeMigratorBase
{
    public override string[] Editors => new[] { "Umbraco.Date", "Umbraco.DateTime" };

    public override string GetDataType(SyncDataTypeInfo dataTypeInfo)
        => "Umbraco.DateTime";

    public override object GetConfigValues(SyncDataTypeInfo dataTypeInfo)
        => dataTypeInfo.MapPreValues(new DateTimeConfiguration());
}
