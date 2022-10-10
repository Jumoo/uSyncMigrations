using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.DataTypes;
internal class MediaPickerMigrator : DataTypeMigratorBase
{
    public override string[] Editors => new[]
    {
        "__Umbraco.MediaPicker2"
    };

    public override string GetDataType(SyncDataTypeInfo dataTypeInfo)
        => "Umbraco.MediaPicker";

    public override object GetConfigValues(SyncDataTypeInfo dataTypeInfo)
    {
        var config = new MediaPickerConfiguration();

        var mappings = new Dictionary<string, string>
        {
            { "multiPicker", nameof(config.Multiple) },
            { "disableFolderSelect", nameof(config.DisableFolderSelect) },
            { "startNodeId", nameof(config.StartNodeId) },
            { "onlyImages", nameof(config.OnlyImages) }
        };

        return dataTypeInfo.MapPreValues(config, mappings);
    }
}


internal class MediaPicker3Migrator : DataTypeMigratorBase
{
    public override string[] Editors => new[]
    {
        "Umbraco.MediaPicker",
        "Umbraco.MediaPicker2",
        "Umbraco.MultipleMediaPicker"
    };

    public override string GetDataType(SyncDataTypeInfo dataTypeInfo)
        => "Umbraco.MediaPicker3";

    public override object GetConfigValues(SyncDataTypeInfo dataTypeInfo)
    {
        var config = new MediaPicker3Configuration()
        {
            Crops = Array.Empty<MediaPicker3Configuration.CropConfiguration>()
        };

        var imageOnly = dataTypeInfo.GetPreValueOrDefault("onlyImages", false);
        if (imageOnly) config.Filter = "Image";

        var mappings = new Dictionary<string, string>
        {
            { "multiPicker", nameof(config.Multiple) },
            { "startNodeId", nameof(config.StartNodeId) },
        };

        return dataTypeInfo.MapPreValues(config, mappings);

    }
}