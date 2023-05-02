using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Umbraco.Cms.Core.Composing;

namespace uSync.Migrations.Configuration.Models;

/// <summary>
///  this is the model used when we load from disk ?
/// </summary>
[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
[HideFromTypeFinder]
public class MigrationPlan : ISyncMigrationPlan
{
    public int Version { get; set; } = 7;

    public string Name { get; set; } = string.Empty;

    public string Icon { get; set; } = "icon-star";

    public string Description { get; set; } = "Loaded from disk";

    public int Order { get; set; } = 100;

    public MigrationOptions Options { get; set; }
        = new MigrationOptions();
}
