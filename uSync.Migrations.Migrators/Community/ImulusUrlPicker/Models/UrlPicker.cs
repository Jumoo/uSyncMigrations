using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace uSync.Migrations.Migrators.Community.ImulusUrlPicker.Models;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class UrlPicker
{
    public UrlPickerValue? Value { get; set; }
}
