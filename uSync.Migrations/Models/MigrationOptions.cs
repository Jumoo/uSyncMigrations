using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace uSync.Migrations.Models;

public class MigrationOptions
{
    public string MigrationType { get; set; }

    public IEnumerable<HandlerOption> Handlers { get; set; }

    public bool BlockListViews { get; set; } = true;

    public bool BlockCommonTypes { get; set; } = true;
}

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class HandlerOption
{
    public string Name { get; set; }

    public bool Include { get; set; } = true;
}
