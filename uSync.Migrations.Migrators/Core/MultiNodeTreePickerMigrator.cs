using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

using uSync.Migrations.Core.Extensions;

namespace uSync.Migrations.Migrators.Core;

[SyncMigrator(UmbEditors.Aliases.MultiNodeTreePicker, IsDefaultAlias = true)]
[SyncMigrator("Umbraco.MultiNodeTreePicker2")]
public class MultiNodeTreePickerMigrator : SyncPropertyMigratorBase
{
    public override string GetDatabaseType(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
=> nameof(ValueStorageType.Ntext);

    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var config = new MultiNodePickerConfiguration();

        var filter = dataTypeProperty.PreValues?.FirstOrDefault(pv => pv.Alias == "filter");
        if (filter != null)
        {
            filter.Value = string.Join(",", filter.Value?.Split(",").Select(x => context.ContentTypes.GetReplacementAlias(x)) ?? Enumerable.Empty<string>());
        }

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
                    values.Add(Udi.Create(context.GetEntityType(guid), guid));
                }
                else if (UdiParser.TryParse<GuidUdi>(item, out var udi) == true)
                {
                    values.Add(udi);
                } 
                else if (int.TryParse(item, out var id) == true) 
                {
                    // Really old editors might have numeric ids
                    var possibleGuid = context.GetKey(id);
                    if (possibleGuid != Guid.Empty)
                    {
                        values.Add(Udi.Create(context.GetEntityType(possibleGuid), possibleGuid));
                    }
                }
            }
        }

        return string.Join(",", values);
    }
}
