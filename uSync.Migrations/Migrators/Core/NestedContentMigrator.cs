using Newtonsoft.Json;

using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Composing;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.NestedContent, typeof(NestedContentConfiguration), IsDefaultAlias = true)]
[SyncMigrator("Our.Umbraco.NestedContent")]
public class NestedContentMigrator : SyncPropertyMigratorBase
{
    Lazy<SyncPropertyMigratorCollection> _migrators;

    public NestedContentMigrator(Lazy<SyncPropertyMigratorCollection> migrators)
    {
        _migrators = migrators;
    }

    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.NestedContent;

    public override object GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => new NestedContentConfiguration().MapPreValues(dataTypeProperty.PreValues);

    // TODO: [KJ] Nested content GetContentValue (so we can recurse)
    public override string GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (string.IsNullOrWhiteSpace(contentProperty.Value)) return string.Empty;

        var rowValues = JsonConvert.DeserializeObject<IList<NestedContentRowValue>>(contentProperty.Value);

        foreach (var row in rowValues)
        {
            // row.ContentTypeAlias

            foreach (var property in row.RawPropertyValues)
            {
                var editorAlias = context.GetEditorAlias(row.ContentTypeAlias, property.Key);
                if (editorAlias == null) continue;

                var migrator = _migrators.Value.Get(editorAlias.OrginalEditorAlias);
                if (migrator != null)
                {
                    row.RawPropertyValues[property.Key] = migrator.GetContentValue(new SyncMigrationContentProperty(row.ContentTypeAlias, property.Value.ToString()), context);
                }
            }
        }

        return JsonConvert.SerializeObject(rowValues, Formatting.Indented);
    }
}

internal class NestedContentRowValue
{
    [JsonProperty("key")]
    public Guid Id { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("ncContentTypeAlias")]
    public string ContentTypeAlias { get; set; } = null!;

    [JsonExtensionData]
    public IDictionary<string, object?> RawPropertyValues { get; set; } = null!;
}
