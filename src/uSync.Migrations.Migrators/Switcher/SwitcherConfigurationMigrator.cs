using Umbraco.Cms.Core;
using Umbraco.Extensions;

using uSync.Core.DataTypes;
using uSync.Migrations.Core.Migrators;

namespace uSync.Migrations.Migrators.Switcher;

public class SwitcherConfigurationMigrator : SyncConfigurationMigratorBase, IConfigurationSerializer
{
    public string Name => nameof(SwitcherConfigurationMigrator);

    public string[] Editors => ["Our.Umbraco.Switcher"];
    public override string? TargetEditor => Constants.PropertyEditors.Aliases.Boolean;

    public override Task<IDictionary<string, object>> GetMigratedConfigurationAsync(string name, IDictionary<string, object> configuration)
    {
        // hideLabel is the opposite of showLabels, so negate it. ToString() covers bool, JsonElement and JsonNode.
        if (configuration.Remove("hideLabel", out var hideLabel))
        {
            var hide = hideLabel?.ToString().TryConvertTo<bool>();
            if (hide?.Success == true)
                configuration["showLabels"] = !hide.Value.Result;
        }

        return Task.FromResult(MigratePropertyNames(configuration, new Dictionary<string, string>
        {
            { "onLabelText", "onLabel" },
            { "offLabelText", "offLabel" },
            { "switchOn", "default" }
        }));
    }
}
