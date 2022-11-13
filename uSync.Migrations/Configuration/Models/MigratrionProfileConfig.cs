using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace uSync.Migrations.Configuration.Models;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class MigratrionProfileConfig
{
    public string[] Remove { get; set; }
    public List<MigrationProfile> Profiles { get; set; }
}
