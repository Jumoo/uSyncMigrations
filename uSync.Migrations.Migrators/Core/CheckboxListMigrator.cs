using Newtonsoft.Json;

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace uSync.Migrations.Migrators.Core;

[SyncMigrator(UmbEditors.Aliases.CheckBoxList)]
public class CheckboxListMigrator : SyncPropertyMigratorBase
{
    public override string GetDatabaseType(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => nameof(ValueStorageType.Nvarchar);

    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var config = new ValueListConfiguration();

        foreach (var item in dataTypeProperty.PreValues ?? Enumerable.Empty<PreValue>())
        {
            config.Items.Add(new ValueListConfiguration.ValueListItem
            {
                Id = item.SortOrder,
                Value = item.Value
            });
        }

        return config;
    }

    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (contentProperty.Value == null) return null;
        if (!contentProperty.Value.DetectIsJson())
            return JsonConvert.SerializeObject(contentProperty.Value.ToDelimitedList(), Formatting.Indented);

        // json stored property. will like be inside another thing 
        // (DTGE, maybe nested??)

        var values = JsonConvert.DeserializeObject<List<string>>(contentProperty.Value);
        if (values == null)
        {
            context.AddMessage(
				this.GetType().Name,
				contentProperty.ContentTypeAlias,
				$"Value is not valid string list [{contentProperty.Value ?? string.Empty}",
				MigrationMessageType.Warning);
            return null;
        }

        var outputValues = new List<string>();
        foreach (var value in values)
        {
            // TODO: Check this logic it seems odd. 
            if (int.TryParse(value, out int intValue))
            {
            }
            else
            {
                outputValues.Add(value);
            }
        }

        return JsonConvert.SerializeObject(outputValues);
    }
}
