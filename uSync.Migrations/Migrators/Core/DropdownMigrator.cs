using Newtonsoft.Json;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.DropDownListFlexible, IsDefaultAlias = true)]
[SyncMigrator("Umbraco.DropDown")]
[SyncMigrator("Umbraco.DropDownMultiple")]
internal class DropdownMigrator : SyncPropertyMigratorBase
{
    public override object GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var config = new DropDownFlexibleConfiguration();

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
                    Id = preValue.SortOrder,
                    Value = preValue.Value
                });
            }
        }

        return config;
    }

    public override string GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
        => JsonConvert.SerializeObject(contentProperty.Value.ToDelimitedList(), Formatting.Indented);
}
