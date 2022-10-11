using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Extensions;

using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

internal class MediaPicker3Migrator : SyncMigratorBase
{
    public override string[] Editors => new[]
    {
        "Umbraco.MediaPicker",
        "Umbraco.MediaPicker2",
        "Umbraco.MultipleMediaPicker"
    };

    public override string GetEditorAlias(string editorAlias, string dabaseType)
        => "Umbraco.MediaPicker3";

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
    {
        var config = new MediaPicker3Configuration()
        {
            Crops = Array.Empty<MediaPicker3Configuration.CropConfiguration>()
        };

        var imageOnly = preValues.GetPreValueOrDefault("onlyImages", false);
        if (imageOnly) config.Filter = "Image";

        var mappings = new Dictionary<string, string>
        {
            { "multiPicker", nameof(config.Multiple) },
            { "startNodeId", nameof(config.StartNodeId) },
        };

        return config.MapPreValues(preValues, mappings);
    }

    public override string GetContentValue(string editorAlias, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;

        var media = new List<ConvertedMediaCrop>();

        var images = value.ToDelimitedList();

        foreach (var image in images)
        {
            var udi = UdiParser.Parse(image) as GuidUdi;

            if (udi != null)
            {
                media.Add(new ConvertedMediaCrop
                {
                    Key = udi.Guid.Increment() // a hack but it means the GUID is constant between syncs.
                    MediaKey = udi.Guid,
                    FocalPoint = new ImageCropperValue.ImageCropperFocalPoint
                    {
                        Left = 0.5M,
                        Top = 0.5M
                    }
                });
            }
        }

        return JsonConvert.SerializeObject(media, Formatting.Indented);
    }


    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    private class ConvertedMediaCrop
    {
        public Guid Key { get; set; }
        public Guid MediaKey { get; set; }

        public IEnumerable<ImageCropperValue.ImageCropperCrop>? Crops { get; set; }
        = new List<ImageCropperValue.ImageCropperCrop>();

        public ImageCropperValue.ImageCropperFocalPoint? FocalPoint { get; set; }
    }
}