using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.DataTypes;
internal class RelatedLinksToMultiUrlPickerMigrator : DataTypeMigratorBase
{
    public override string[] Editors => new[] {
        "Umbraco.RelatedLinks",
        "Umbraco.RelatedLinks2"
    };

    public override string GetDataType(SyncDataTypeInfo dataTypeInfo)
        => "Umbraco.MultiUrlPicker";

    public override object GetConfigValues(SyncDataTypeInfo dataTypeInfo)
    {
        var config = new MultiUrlPickerConfiguration();
        var maxValue = dataTypeInfo.GetPreValueOrDefault("max", -1);
        if (maxValue != -1) config.MaxNumber = maxValue;

        return config;
    }
}
