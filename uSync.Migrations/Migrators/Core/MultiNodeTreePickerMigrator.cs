using Newtonsoft.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;
using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

internal class MultiNodeTreePickerMigrator : SyncPropertyMigratorBase
{
    public override string[] Editors => new[]
    {
        UmbConstants.PropertyEditors.Aliases.MultiNodeTreePicker,
        "Umbraco.MultiNodeTreePicker2",
    };

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues, SyncMigrationContext context)
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

        return config.MapPreValues(preValues, mappings);
    }

    public override string GetContentValue(string editorAlias, string value, SyncMigrationContext context)
    {
        var values = new List<Udi>();

        var items = value?.ToDelimitedList();

        if (items?.Any() == true)
        {
            foreach (var item in items)
            {
                // Test if the value is already a Guid, if not, test if it's a GuidUdi instead.
                // If it is a Guid, then `guid` will be assigned and continue on.
                if (Guid.TryParse(item, out Guid guid) == false && UdiParser.TryParse<GuidUdi>(item, out var udi) == true)
                {
                    guid = udi.Guid;
                }

                if (guid.Equals(Guid.Empty) == false)
                {
                    // TODO: Eeek! This might possibly be content, media or member! [LK]
                    values.Add(Udi.Create(UmbConstants.UdiEntityType.Document, guid));
                }
            }
        }

        return JsonConvert.SerializeObject(values);
    }
}
