using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace uSync.Migrations.Migrators.Community.ImulusUrlPicker.Models
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UrlPickerTypeData
    {
        public string? Url { get; set; }
        public string? ContentId { get; set; }
        public string? MediaId { get; set; }

    }
}
