using Newtonsoft.Json;

using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

using uSync.Migrations.Core.Extensions;

namespace uSync.Migrations.Migrators.Core;

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

        var contentTypeKeys = config.ContentTypes.Select(x => x.Alias)
            .WhereNotNull() // satisfy nullability requirement
            .Select(context.ContentTypes.GetReplacementAlias)
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Select(b => context.ContentTypes.TryGetKeyByAlias(b, out var key) ? key : Guid.Empty)
            .Where(x => x != Guid.Empty);

        context.ContentTypes.AddElementTypes(contentTypeKeys, true);

        return config;
    }

    // TODO: [KJ] Nested content GetContentValue (so we can recurse)
    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (string.IsNullOrWhiteSpace(contentProperty.Value)) return string.Empty;

        var rowValues = JsonConvert.DeserializeObject<IList<NestedContentRowValue>>(contentProperty.Value, new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None });
        if (rowValues == null) return string.Empty;

        foreach (var row in rowValues)
        {
            if (row.Id == default)
            {
                row.Id = Guid.NewGuid();
            }

            foreach (var property in row.RawPropertyValues)
            {
                var contentTypeAlias = context.ContentTypes.GetReplacementAlias(row.ContentTypeAlias);
                var propertyAlias = context.ContentTypes.GetReplacementAlias(property.Key);

                if (context.ContentTypes.TryGetEditorAliasByTypeAndProperty(contentTypeAlias, propertyAlias, out var editorAlias) is false) { continue; }

                try
                {
                    if (context.Migrators.TryGetMigrator($"{contentProperty.ContentTypeAlias}_{contentProperty.PropertyAlias}", editorAlias.OriginalEditorAlias, out var migrator) is false) { continue; }

                    row.RawPropertyValues[property.Key] = migrator.GetContentValue(
                        new SyncMigrationContentProperty(
                            contentTypeAlias, property.Key, contentTypeAlias, property.Value?.ToString()),
                            context);

                }
                catch (Exception ex)
                {
                    throw new Exception($"Nested Error: [{editorAlias.OriginalEditorAlias} -{property.Key}] : {ex.Message}", ex);
                }
            }
        }

        return JsonConvert.SerializeObject(rowValues, Formatting.Indented);
    }
}

public class NestedContentRowValue
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
