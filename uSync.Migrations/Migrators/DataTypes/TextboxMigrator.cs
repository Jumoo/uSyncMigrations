using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.DataTypes;
public class TextboxMigrator : DataTypeMigratorBase
{
    public override string[] Editors => new[] { "Umbraco.Textbox" };

    public override string GetDataType(SyncDataTypeInfo dataTypeInfo)
        => "Umbraco.TextBox";

    public override object GetConfigValues(SyncDataTypeInfo dataTypeInfo)
        => dataTypeInfo.MapPreValues(new TextboxConfiguration());
}
