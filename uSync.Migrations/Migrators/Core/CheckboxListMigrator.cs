using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

internal class CheckboxListMigrator : SyncPropertyMigratorBase
{
    public override string[] Editors => new[] { UmbConstants.PropertyEditors.Aliases.CheckBoxList };

    public override string GetDatabaseType(string editorAlias, string databaseType, SyncMigrationContext context)
        => nameof(ValueStorageType.Nvarchar);

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues, SyncMigrationContext context)
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
