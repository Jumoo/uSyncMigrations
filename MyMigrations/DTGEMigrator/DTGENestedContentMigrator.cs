using Newtonsoft.Json;
using NPoco.fastJSON;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;
using uSync.Migrations.Composing;
using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators;
using uSync.Migrations.Migrators.Models;

namespace MyMigrations.Migrators;

[SyncMigrator("DTGE." + Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.NestedContent, typeof(NestedContentConfiguration), IsDefaultAlias = true)]
[SyncMigrator("DTGE.Our.Umbraco.NestedContent")]
[SyncDefaultMigrator]
public class DTGENestedContentMigrator : NestedContentMigrator
{
    public DTGENestedContentMigrator()
    { }

    
    // TODO: [KJ] Nested content GetContentValue (so we can recurse)
    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (string.IsNullOrWhiteSpace(contentProperty.Value)) return string.Empty;

        var rowValues = JsonConvert.DeserializeObject<IList<NestedContentRowValue>>(contentProperty.Value);
        if (rowValues == null) return string.Empty;

        foreach (var row in rowValues)
        {
            if (row.Id == default)
            {
                row.Id = Guid.NewGuid();
            }

            foreach (var property in row.RawPropertyValues)
            {
                var editorAlias = context.ContentTypes.GetEditorAliasByTypeAndProperty(row.ContentTypeAlias, property.Key);
                if (editorAlias == null) continue;

                try
                {
                    var migrator = context.Migrators.TryGetMigrator("DTGE." + editorAlias.OriginalEditorAlias);
                    if (migrator == null)
                        migrator = context.Migrators.TryGetMigrator(editorAlias.OriginalEditorAlias);
                    
                    if (migrator != null)
                    {
                        var contentValue = migrator.GetContentValue(
                            new SyncMigrationContentProperty(
                                row.ContentTypeAlias, property.Key, row.ContentTypeAlias, property.Value?.ToString()),
                            context);
                        if (contentValue.DetectIsJson())
                            row.RawPropertyValues[property.Key] = JsonConvert.DeserializeObject(contentValue);
                        else
                            row.RawPropertyValues[property.Key] = contentValue;
                    }
                }
                catch(Exception ex)
                {
                    throw new Exception($"Nested Error: [{editorAlias.OriginalEditorAlias} -{property.Key}] : {ex.Message}", ex);
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

