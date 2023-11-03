using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace uSync.Migrations.Migrators.Community.ImulusUrlPicker.Models;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class UrlPickerMeta
{
    public string? Title { get; set; }
    public string? NewWindow { get; set; }
}
