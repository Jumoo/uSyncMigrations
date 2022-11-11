using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.CheckBoxList)]
internal class CheckboxListMigrator : SyncPropertyMigratorBase
{
    public override string GetDatabaseType(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => nameof(ValueStorageType.Nvarchar);

    public override object GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var config = new ValueListConfiguration();

        foreach (var item in dataTypeProperty.PreValues)
        {
            config.Items.Add(new ValueListConfiguration.ValueListItem
            {
                Id = item.SortOrder,
                Value = item.Value
            });
        }

        return config;
    }

    public override string GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
        => string.IsNullOrWhiteSpace(contentProperty.Value)
            ? contentProperty.Value 
            : string.Join(",\r\n", contentProperty.Value.ToDelimitedList());
}
