using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Extensions;
using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

internal class MediaPickerMigrator : SyncPropertyMigratorBase
{
    public override string[] Editors => new[]
    {
        UmbConstants.PropertyEditors.Aliases.MediaPicker,
        "Umbraco.MediaPicker2",
        UmbConstants.PropertyEditors.Aliases.MultipleMediaPicker,
    };

    public override string GetEditorAlias(string editorAlias, string databaseType, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.MediaPicker3;

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues, SyncMigrationContext context)
    {
        var config = new MediaPicker3Configuration()
        {
            Crops = Array.Empty<MediaPicker3Configuration.CropConfiguration>()
        };

        var imageOnly = preValues.GetPreValueOrDefault("onlyImages", false);
        if (imageOnly) config.Filter = UmbConstants.Conventions.MediaTypes.Image;

        var mappings = new Dictionary<string, string>
        {
            { "multiPicker", nameof(config.Multiple) },
            { "startNodeId", nameof(config.StartNodeId) },
        };

        return config.MapPreValues(preValues, mappings);
    }

    public override string GetContentValue(string editorAlias, string value, SyncMigrationContext context)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var defaultCrops = Enumerable.Empty<ImageCropperValue.ImageCropperCrop>();
        var defaultFocalPoint = default(ImageCropperValue.ImageCropperFocalPoint);

        var media = new List<MediaWithCropsDto>();

        var images = value.ToDelimitedList();

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
                    Key = guid.Combine(value.ToGuid()), // guid.Increment(), // a hack but it means the GUID is constant between syncs.
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