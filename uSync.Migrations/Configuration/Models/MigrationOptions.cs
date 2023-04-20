using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace uSync.Migrations.Configuration.Models;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class MigrationOptions
{
    public string Group { get; set; } = "legacy";
    public string Source { get; set; } = "uSync/data";
    public string Target { get; set; } = "uSync/migrated";

    public int SourceVersion { get; set; } = 7;

    public string? MigrationType { get; set; }

    public IList<HandlerOption>? Handlers { get; set; }

    public IDictionary<string, string>? PreferredMigrators { get; set; }

    public bool BlockListViews { get; set; } = true;

    public bool BlockCommonTypes { get; set; } = true;

    /// <summary>
    ///  items that you want to block by type
    /// </summary>
    public Dictionary<string, List<string>>? BlockedItems { get; set; }

    /// <summary>
    ///  Blocked properties use (alias of somethign to block.) syntax?. 
    /// </summary>
    public List<string>? IgnoredProperties { get; set; }

    public Dictionary<string, List<string>>? IgnoredPropertiesByContentType { get; set; }

    /// <summary>
    /// List of tabs that will be changed
    /// </summary>
    public List<TabOptions>? ChangeTabs { get; set; }

    public string? ArchetypeMigrationConfigurer { get; set; }
}

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class HandlerOption
{
    public string Name { get; set; } = string.Empty;

    public bool Include { get; set; } = true;
}
