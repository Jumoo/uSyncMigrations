using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace uSync.Migrations.Configuration.Models;


[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class MigrationOptions
{
    public string Source { get; set; } = "uSync/data";
    public string Target { get; set; } = "uSync/migrated";
    public string MigrationType { get; set; }

    public IEnumerable<HandlerOption> Handlers { get; set; }

    public bool BlockListViews { get; set; } = true;

    public bool BlockCommonTypes { get; set; } = true;

    /// <summary>
    ///  list of property aliases not to import in contenttype/content items
    /// </summary>
    public List<string> BlockedProperties { get; set; }


}

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class HandlerOption
{
    public string Name { get; set; }

    public bool Include { get; set; } = true;
}
