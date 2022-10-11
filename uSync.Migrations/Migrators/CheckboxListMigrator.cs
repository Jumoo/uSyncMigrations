using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;
internal class CheckboxListMigrator : SyncMigratorBase
{
    public override string[] Editors => new[] {
        "Umbraco.CheckBoxList",
        "Umbraco.RadioButtonList"
    };

    public override string GetDatabaseType(string editorAlias, string databaseType)
        => "Nvarchar";

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
    {
        var config = new ValueListConfiguration();

        foreach (var item in preValues)
        {
            config.Items.Add(new ValueListConfiguration.ValueListItem
            {
                Id = item.SortOrder,
                Value = item.Value
            });
        }

        return config;
    }
}
