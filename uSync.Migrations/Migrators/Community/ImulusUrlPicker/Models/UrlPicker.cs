using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace uSync.Migrations.Migrators.Community.ImulusUrlPicker.Models
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UrlPicker
    {
        public UrlPickerValue? Value { get; set; }
    }
}
