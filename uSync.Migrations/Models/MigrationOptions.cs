using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace uSync.Migrations.Models;


[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class MigrationProfileInfo
{
    public List<MigrationProfile> Profiles { get; set; }

    public bool HasCustom { get; set; }
}

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class MigratrionProfileConfig
{
    public string[] Remove { get; set; }
    public List<MigrationProfile> Profiles { get; set; }
}


[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class MigrationProfile
{
    public string Name { get; set; }
    public string Icon { get; set; }
    public string Description { get; set; }

    public MigrationOptions Options { get; set; }
        = new MigrationOptions();
}


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
