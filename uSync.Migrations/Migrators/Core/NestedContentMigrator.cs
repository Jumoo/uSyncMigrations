using Newtonsoft.Json;

using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Composing;
using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.NestedContent, typeof(NestedContentConfiguration), IsDefaultAlias = true)]
[SyncMigrator("Our.Umbraco.NestedContent")]
[SyncDefaultMigrator]
public class NestedContentMigrator : SyncPropertyMigratorBase
{
    public NestedContentMigrator()
    { }

    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.NestedContent;

    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        if (dataTypeProperty?.PreValues == null) return new NestedContentConfiguration();

        var config = (NestedContentConfiguration?)new NestedContentConfiguration().MapPreValues(dataTypeProperty.PreValues);
        if (config?.ContentTypes == null) return new NestedContentConfiguration();

        foreach(var contentTypeAlias in config.ContentTypes.Select(x => x.Alias))
        {
            if (string.IsNullOrWhiteSpace(contentTypeAlias)) continue;

            var key = context.ContentTypes.GetKeyByAlias(contentTypeAlias);
            context.ContentTypes.AddElementType(key);
        }

        return config;
    }

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
                    var migrator = context.Migrators.TryGetMigrator(editorAlias.OriginalEditorAlias);
                    if (migrator != null)
                    {
                        row.RawPropertyValues[property.Key] = migrator.GetContentValue(
                            new SyncMigrationContentProperty(
                                row.ContentTypeAlias, property.Key, row.ContentTypeAlias, property.Value?.ToString()),
                                context);
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
