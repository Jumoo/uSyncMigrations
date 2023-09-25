using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace uSync.Migrations.Migrators.Models.UrlPicker
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UrlPickerValue
    {
        public UrlPickerTypes Type { get; set; }
        public UrlPickerMeta? Meta { get; set; }
        public UrlPickerTypeData? TypeData { get; set; }
    }
}
