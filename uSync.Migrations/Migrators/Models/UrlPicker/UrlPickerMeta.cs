using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace uSync.Migrations.Migrators.Models.UrlPicker
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UrlPickerMeta
    {
        public string? Title { get; set; }
        public string? NewWindow { get; set; }
    }
}
