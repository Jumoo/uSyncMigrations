using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace uSync.Migrations.Configuration.Models;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class MigrationPlanInfo
{
    public List<ISyncMigrationPlan> Plans { get; set; }
        = new List<ISyncMigrationPlan>();

    public bool HasCustom { get; set; }
}
