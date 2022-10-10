using System.Collections.Specialized;

using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.DataTypes;

internal class ContentPickerMigration : DataTypeMigratorBase
{
    public override string[] Editors => new[]
    {
        "Umbraco.ContentPickerAlias",
        "Umbraco.ContentPicker2"
    };

    public override string GetDataType(SyncDataTypeInfo dataTypeInfo)
        => "Umbraco.ContentPicker";

    public override object GetConfigValues(SyncDataTypeInfo dataTypeInfo)
    {
        var config = new ContentPickerConfiguration();

        var mappings = new Dictionary<string, string>
        {
            { "showOpenButton", nameof(config.ShowOpenButton) },
            { "startNodeId", nameof(config.StartNodeId) }
        };

        return dataTypeInfo.MapPreValues(config, mappings);
    }
}

internal class MultiNodeTreePicker : DataTypeMigratorBase
{
    public override string[] Editors => new[]
    {
        "Umbraco.MultiNodeTreePicker2"
    };

    public override string GetDataType(SyncDataTypeInfo dataTypeInfo)
        => "Umbraco.MultiNodeTreePicker";

    public override object GetConfigValues(SyncDataTypeInfo dataTypeInfo)
    {
        var config = new MultiNodePickerConfiguration();
        var mappings = new Dictionary<string, string>
        {
            { "ignoreUserStartNodes", nameof(config.IgnoreUserStartNodes) },
            { "startNode", nameof(config.TreeSource) },
            { "filter", nameof(config.Filter) },
            { "minNumber", nameof(config.MinNumber) },
            { "maxNumber", nameof(config.MaxNumber) },
            { "showOpenButton", nameof(config.ShowOpen) }
        };

        return dataTypeInfo.MapPreValues(config, mappings);
    }
}
