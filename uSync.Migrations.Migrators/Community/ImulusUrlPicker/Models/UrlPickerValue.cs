using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace uSync.Migrations.Migrators.Community.ImulusUrlPicker.Models;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class UrlPickerValue
{
    public UrlPickerTypes Type { get; set; }
    public UrlPickerMeta? Meta { get; set; }
    public UrlPickerTypeData? TypeData { get; set; }
}
