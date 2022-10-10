using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.DataTypes;

public class TextAreaMigrator : DataTypeMigratorBase
{
    public override string[] Editors => new[]
    {
        "Umbraco.TextArea",
        "Umbraco.TextboxMultiple"
    };

    public override string GetDataType(SyncDataTypeInfo dataTypeInfo)
        => "Umbraco.TextArea";

    public override object GetConfigValues(SyncDataTypeInfo dataTypeInfo)
        => dataTypeInfo.MapPreValues(new TextAreaConfiguration());
}
