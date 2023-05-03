using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;
using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.MultiNodeTreePicker, IsDefaultAlias = true)]
[SyncMigrator("Umbraco.MultiNodeTreePicker2")]
public class MultiNodeTreePickerMigrator : SyncPropertyMigratorBase
{
    public override string GetDatabaseType(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
=> nameof(ValueStorageType.Ntext);

    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var config = new MultiNodePickerConfiguration();

        var mappings = new Dictionary<string, string>
        {
            { "filter", nameof(config.Filter) },
            { "ignoreUserStartNodes", nameof(config.IgnoreUserStartNodes) },
            { "minNumber", nameof(config.MinNumber) },
            { "maxNumber", nameof(config.MaxNumber) },
            { "showOpenButton", nameof(config.ShowOpen) },
            { "startNode", nameof(config.TreeSource) },
        };

        return config.MapPreValues(dataTypeProperty.PreValues, mappings);
    }

    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        var values = new List<Udi>();

        var items = contentProperty.Value?.ToDelimitedList();

        if (items?.Any() == true)
        {
            foreach (var item in items)
            {
                if (Guid.TryParse(item, out var guid) == true)
                {
                    // TODO: Eeek! This might possibly be content, media or member! [LK]
                    values.Add(Udi.Create(UmbConstants.UdiEntityType.Document, guid));
                }
                else if (UdiParser.TryParse<GuidUdi>(item, out var udi) == true)
                {
                    values.Add(udi);
                }
            }
        }

        return string.Join(",", values);
    }
}
