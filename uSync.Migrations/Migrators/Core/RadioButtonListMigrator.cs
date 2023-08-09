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

        if (dataTypeProperty.PreValues is not null)
        {
            foreach (var item in dataTypeProperty.PreValues.OrderBy(x => x.SortOrder))
            {
                config.Items.Add(new ValueListConfiguration.ValueListItem
                {
                    Id = int.TryParse(item.Alias, out var id) == true ? id : item.SortOrder,
                    Value = item.Value
                });
            }

            context.Migrators.AddCustomValues(
                $"dataType_{dataTypeProperty.DataTypeAlias}_items",
                config.Items.ToDictionary(x => x.Id.ToString(), x => (object)x.Value!));
        }

        return config;
    }

    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (string.IsNullOrWhiteSpace(contentProperty.Value) == false)
        {
            var dataTypeAlias = context.ContentTypes.GetDataTypeAlias(contentProperty.ContentTypeAlias, contentProperty.PropertyAlias);
            var items = context.Migrators.GetCustomValues($"dataType_{dataTypeAlias}_items");
            if (items?.TryGetValue(contentProperty.Value, out var value) == true && value is string str)
            {
                return str;
            }
        }

        return base.GetContentValue(contentProperty, context);
    }
}
