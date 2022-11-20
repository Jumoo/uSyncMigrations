using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Umbraco.Cms.Core.Composing;

namespace uSync.Migrations.Configuration.Models;

/// <summary>
///  this is the model used when we load from disk ?
/// </summary>
[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
[HideFromTypeFinder]
public class MigrationProfile : ISyncMigrationProfile
{
    public string Name { get; set; }
    public string Icon { get; set; }
    public string Description { get; set; }

    public int Order { get; set; } = 100;

    public MigrationOptions Options { get; set; }
        = new MigrationOptions();
}
