using Newtonsoft.Json;

using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace uSync.Migrations.Migrators.Core;

[SyncMigrator(UmbEditors.Aliases.DropDownListFlexible, IsDefaultAlias = true)]
[SyncMigrator("Umbraco.DropDown")]
[SyncMigrator("Umbraco.DropDownMultiple")]
public class DropdownMigrator : SyncPropertyMigratorBase
{
    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var config = new DropDownFlexibleConfiguration();
        if (dataTypeProperty.PreValues == null) return config;

        var index = 0;
        foreach (var preValue in dataTypeProperty.PreValues)
        {
            if (preValue.Alias.InvariantEquals("multiple"))
            {
                var attempt = preValue.Value.TryConvertTo<bool>();
                if (attempt.Success)
                {
                    config.Multiple = attempt.Result;
                }
            }
            else
            {
                config.Items.Add(new ValueListConfiguration.ValueListItem
                {
                    Id = index,
                    Value = preValue.Value
                });
            }
            index++;
        }

        return config;
    }

    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
        => contentProperty.Value == null
            ? null
            : JsonConvert.SerializeObject(contentProperty.Value.ToDelimitedList(), Formatting.Indented);
}
