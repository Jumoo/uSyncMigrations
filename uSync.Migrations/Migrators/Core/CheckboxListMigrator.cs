using Newtonsoft.Json;

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;
using uSync.Migrations.Context;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.CheckBoxList)]
public class CheckboxListMigrator : SyncPropertyMigratorBase
{
    public override string GetDatabaseType(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => nameof(ValueStorageType.Nvarchar);

    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var config = new ValueListConfiguration();

        foreach (var item in dataTypeProperty.PreValues ?? Enumerable.Empty<Migrations.Models.PreValue>())
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
        if (values == null) return null;

        var outputValues = new List<string>();
        foreach(var value in values)
        {
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
