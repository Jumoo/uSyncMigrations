using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using uSync.Migrations.Models;

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

    /// <summary>
    ///  migrators by property name. 
    /// </summary>
    /// <remarks>
    ///  property migrators will be searched by property alias
    ///  and contentType_propertyAlias, so you can define both 
    ///  for all properties with a name, or restrict to the content/media type
    /// </remarks>
    public IDictionary<string, string>? PropertyMigrators { get; set; }

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

    /// <summary>
    ///  things we might want to merge. 
    /// </summary>
    public Dictionary<string, MergingPropertiesConfig> MergingProperties { get; set; } = new(StringComparer.InvariantCultureIgnoreCase);

}

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class HandlerOption
{
    public string Name { get; set; } = string.Empty;

    public bool Include { get; set; } = true;
}
