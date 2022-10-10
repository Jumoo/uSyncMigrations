using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Extensions;

namespace uSync.Migrations.Migrators.Content;
internal class MediaToMediaPicker3ValueMigrator : ISyncContentPropertyMigrator
{
    public string[] Editors => new[]
    {
        "Umbraco.MediaPicker2"
    };

    public string GetMigratedValue(string editorAlias, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;

        var media = new List<ConvertedMediaCrop>();

        var images = value.ToDelimitedList();

        foreach(var image in images)
        {
            var udi = UdiParser.Parse(image) as GuidUdi;

            if (udi != null) {
                media.Add(new ConvertedMediaCrop
                {
                    Key = Guid.NewGuid(),
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
