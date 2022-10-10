using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.DataTypes;
internal class RichTextBoxMigrator : DataTypeMigratorBase
{
    public override string[] Editors => new[] { "Umbraco.TinyMCEv3" };

    public override string GetDataType(SyncDataTypeInfo dataTypeInfo)
        => "Umbraco.TinyMCE";

    public override object GetConfigValues(SyncDataTypeInfo dataTypeInfo)
        => dataTypeInfo.MapPreValues(new RichTextConfiguration());
}
