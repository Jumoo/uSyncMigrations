using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace uSync.Migrations.Models;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class MigrationResults
{
    public bool Success { get; set; }
    public Guid MigrationId { get; set; }
    public IEnumerable<MigrationMessage> Messages { get; set; }
        = new List<MigrationMessage>();
}