using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Umbraco.Cms.Core.Composing;

namespace uSync.Migrations.Configuration.Models;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public interface ISyncMigrationProfile : IDiscoverable
{
    int Order { get; }

    string Name { get; }
    string Icon { get; }
    string Description { get; }
    MigrationOptions Options { get; }
}
