using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace uSync.Migrations.Migrators.Community.ImulusUrlPicker.Models
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UrlPickerMeta
    {
        public string? Title { get; set; }
        public string? NewWindow { get; set; }
    }
}
