using Newtonsoft.Json;

using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;
internal class DropdownMigrator : SyncMigratorBase
{
    public override string[] Editors => new[]
    {
        "Umbraco.DropDown.Flexible",
        "Umbraco.DropDown",
        "Umbraco.DropDownMultiple"
    };

    public override string GetEditorAlias(string editorAlias, string dabaseType)
        => "Umbraco.DropDown.Flexible";

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
    {
        var config = new DropDownFlexibleConfiguration();

        foreach (var preValue in preValues)
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

    public override string GetContentValue(string editorAlias, string value)
        => JsonConvert.SerializeObject(value.ToDelimitedList(), Formatting.Indented);
}
