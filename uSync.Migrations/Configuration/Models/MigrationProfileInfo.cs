using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace uSync.Migrations.Configuration.Models;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class MigrationProfileInfo
{
    public List<ISyncMigrationProfile> Profiles { get; set; }

    public bool HasCustom { get; set; }
}
