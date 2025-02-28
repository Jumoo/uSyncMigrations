using Newtonsoft.Json;

using Umbraco.Extensions;

using uSync.Migrations.Core;
using uSync.Migrations.Core.Context;
using uSync.Migrations.Core.Legacy;
using uSync.Migrations.Core.Migrators;
using uSync.Migrations.Core.Migrators.Models;
using uSync.Migrations.Migrators.Core;

namespace MyMigrations.DTGEMigrator;

[SyncMigrator("DTGE." + uSyncMigrations.EditorAliases.NestedContent, typeof(LegacyNestedContentConfiguration), IsDefaultAlias = true)]
[SyncMigrator("DTGE." + uSyncMigrations.EditorAliases.NestedContentCommunity)]
[SyncDefaultMigrator]
public class DTGENestedContentMigrator : NestedContentMigrator
{
    public DTGENestedContentMigrator()
    { }


    // TODO: [KJ] Nested content GetContentValue (so we can recurse)
    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (string.IsNullOrWhiteSpace(contentProperty.Value)) return string.Empty;

        var rowValues = JsonConvert.DeserializeObject<IList<LegacyNestedContentRowValue>>(contentProperty.Value);
        if (rowValues == null) return string.Empty;

        foreach (var row in rowValues)
        {
            if (row.Id == default)
            {
                row.Id = Guid.NewGuid();
            }

            foreach (var property in row.RawPropertyValues)
            {
                if (context.ContentTypes.TryGetEditorAliasByTypeAndProperty(row.ContentTypeAlias, property.Key, out var editorAlias) is false) { continue; }

                try
                {
                    if (context.Migrators.TryGetMigrator("DTGE." + editorAlias.OriginalEditorAlias, out var migrator) is false &&
                        context.Migrators.TryGetMigrator(editorAlias.OriginalEditorAlias, out migrator) is false)
                    {
                        continue;
                    }

                    var contentValue = migrator.GetContentValue(
                        new SyncMigrationContentProperty(
                                contentTypeAlias: row.ContentTypeAlias,
                                propertyAlias: property.Key,
                                editorAlias: row.ContentTypeAlias,
                                value: property.Value?.ToString()),
                        context);

                    if (contentValue?.DetectIsJson() == true)
                        row.RawPropertyValues[property.Key] = JsonConvert.DeserializeObject(contentValue);
                    else
                        row.RawPropertyValues[property.Key] = contentValue;
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

