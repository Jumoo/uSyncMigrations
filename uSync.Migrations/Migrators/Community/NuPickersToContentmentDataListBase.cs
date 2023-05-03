using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Extensions;
using uSync.Migrations.Context;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators.Community;

[SyncMigrator("nuPickers - base migrator, not used directly")]
public class NuPickersToContentmentDataListBase : SyncPropertyMigratorBase
{
    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => "Umbraco.Community.Contentment.DataList";

    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (string.IsNullOrWhiteSpace(contentProperty.Value))
        {
            return string.Empty;
        }

        if (!contentProperty.Value.DetectIsJson())
        {
            return contentProperty.Value;
        }

        var jtoken = JToken.Parse(contentProperty.Value);
        var values = jtoken.Select(t => t.Value<string>("key")).ToArray();
        return JsonConvert.SerializeObject(values, Formatting.Indented);
    }
}