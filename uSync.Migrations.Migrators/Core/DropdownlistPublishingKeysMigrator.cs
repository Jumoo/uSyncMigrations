using Newtonsoft.Json;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;

namespace uSync.Migrations.Migrators.Core;

[SyncMigrator("Umbraco.DropdownlistPublishingKeys")]
public class DropdownlistPublishingKeysMigrator : SyncPropertyMigratorBase
{
    public override string GetDatabaseType(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => nameof(ValueStorageType.Nvarchar).ToLower();

    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.DropDownListFlexible;

    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty,
        SyncMigrationContext context)
    {
        var config = new DropDownFlexibleConfiguration();

        if (dataTypeProperty.PreValues is null)
            return config;

        int index = 0;

        foreach (var preValue in dataTypeProperty.PreValues)
        {
            config.Items.Add(new ValueListConfiguration.ValueListItem
            {
                Id = index++,
                Value = preValue.Value,
            });
        }

        context.Migrators.AddCustomValues(
            $"dataType_{dataTypeProperty.DataTypeAlias}_items",
            config.Items.ToDictionary(x => x.Id.ToString(), x => (object)x.Value!));

        return config;
    }

    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (string.IsNullOrEmpty(contentProperty.Value)) 
            return "[]";
        
        var dataTypeAlias = context.ContentTypes
            .GetDataTypeAlias(contentProperty.ContentTypeAlias, contentProperty.PropertyAlias);

        var items = context.Migrators.GetCustomValues($"dataType_{dataTypeAlias}_items");

        if (items.TryGetValue(contentProperty.Value, out var value) && value is string stringValue)
            return JsonConvert.SerializeObject(new[] { stringValue });

        return "[]";
    }
}
