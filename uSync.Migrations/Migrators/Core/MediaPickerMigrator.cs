using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Extensions;
using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.MediaPicker)]
[SyncMigrator("Umbraco.MediaPicker2")]
[SyncMigrator(UmbConstants.PropertyEditors.Aliases.MultipleMediaPicker)]
public class MediaPickerMigrator : SyncPropertyMigratorBase
{
    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.MediaPicker3;

    public override string GetDatabaseType(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => "Ntext";

    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var config = new MediaPicker3Configuration()
        {
            Crops = Array.Empty<MediaPicker3Configuration.CropConfiguration>()
        };

        var imageOnly = dataTypeProperty.PreValues.GetPreValueOrDefault("onlyImages", false);
        if (imageOnly) config.Filter = UmbConstants.Conventions.MediaTypes.Image;

        var mappings = new Dictionary<string, string>
        {
            { "multiPicker", nameof(config.Multiple) },
            { "startNodeId", nameof(config.StartNodeId) },
        };

        return config.MapPreValues(dataTypeProperty.PreValues, mappings);
    }

    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (string.IsNullOrWhiteSpace(contentProperty.Value))
        {
            return contentProperty.Value;
        }

        var defaultCrops = Enumerable.Empty<ImageCropperValue.ImageCropperCrop>();
        var defaultFocalPoint = default(ImageCropperValue.ImageCropperFocalPoint);

        var media = new List<MediaWithCropsDto>();

        var images = contentProperty.Value.ToDelimitedList();

        foreach (var image in images)
        {
            // Test if the value is already a Guid, if not, test if it's a GuidUdi instead.
            // If it is a Guid, then `guid` will be assigned and continue on.
            if (Guid.TryParse(image, out Guid guid) == false && UdiParser.TryParse<GuidUdi>(image, out var udi) == true)
            {
                guid = udi.Guid;
            }
         
            if (guid.Equals(Guid.Empty) == false)
            {
                media.Add(new MediaWithCropsDto
                {
                    Key = guid.Combine(contentProperty.Value.ToGuid()), // guid.Increment(), // a hack but it means the GUID is constant between syncs.
                    MediaKey = guid,
                    Crops = defaultCrops,
                    FocalPoint = defaultFocalPoint
                });
            }
        }

        return JsonConvert.SerializeObject(media, Formatting.Indented);
    }

    /* This source code has been copied from Umbraco CMS.
     * https://github.com/umbraco/Umbraco-CMS/blob/release-10.2.1/src/Umbraco.Infrastructure/PropertyEditors/MediaPicker3PropertyEditor.cs#L176
     * Modified under the permissions of the MIT License.*/
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    private class MediaWithCropsDto
    {
        public Guid Key { get; set; }

        public Guid MediaKey { get; set; }

        public IEnumerable<ImageCropperValue.ImageCropperCrop>? Crops { get; set; }

        public ImageCropperValue.ImageCropperFocalPoint? FocalPoint { get; set; }
    }
}