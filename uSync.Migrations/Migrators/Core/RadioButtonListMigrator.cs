using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Context;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.RadioButtonList)]
public class RadioButtonListMigrator : SyncPropertyMigratorBase
{
    public override string GetDatabaseType(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => nameof(ValueStorageType.Nvarchar);

    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var config = new ValueListConfiguration();
        if (dataTypeProperty.PreValues == null) return config;

        // Default the order to the sort order of the prevalues
        var preValues = dataTypeProperty.PreValues.OrderBy(x => x.SortOrder);
        
        // Start a counter as we can't rely on the sort order entirely
        var count = 0;

        foreach (var item in preValues)
        {
            config.Items.Add(new ValueListConfiguration.ValueListItem
            {
                Id = count, // Using the count ensure it is unique
                Value = item.Value
            });
            
            // Increment the counter before the next iteration
            count++;
        }

        return config;
    }
}
