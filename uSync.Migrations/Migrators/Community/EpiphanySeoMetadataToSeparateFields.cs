using Newtonsoft.Json;
using Umbraco.Extensions;
using uSync.Migrations.Context;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators.Community;

[SyncMigrator("Epiphany.SeoMetadata")]
[SyncDefaultMigrator]
public class EpiphanySeoMetadataToSeparateFields : SyncPropertyMigratorBase, ISyncPropertySplittingMigrator
{
    private readonly Dictionary<string, (string Name, string Alias, string EditorAlias, string OriginalEditorAlias, Guid DefaultDefinition)> Properties = new()
    {
        { 
            nameof(SeoMetadata.Title), 
            ("Meta title", "metaTitle", "Umbraco.TextBox", "Umbraco.Textbox", Guid.Parse("0cc0eba1-9960-42c9-bf9b-60e150b429ae")) 
        },
        { 
            nameof(SeoMetadata.Description), 
            ("Meta description", "metaDescription", "Umbraco.TextArea", "Umbraco.TextboxMultiple", Guid.Parse("c6bac0dd-4ab9-45b1-8e30-e4b619ee5da3")) 
        },
        { 
            nameof(SeoMetadata.NoIndex),
            ("Robots NoIndex", "robotsNoIndex", "Umbraco.TrueFalse", "Umbraco.TrueFalse", Guid.Parse("92897bc6-a5f3-4ffe-ae27-f2e7e33dda49"))
        },
        { 
            nameof(SeoMetadata.UrlName),
            ("URL name", "umbracoUrlName", "Umbraco.TextBox", "Umbraco.Textbox", Guid.Parse("0cc0eba1-9960-42c9-bf9b-60e150b429ae"))
        },
    };

    public IEnumerable<SplitPropertyContent> GetContentValues(SyncMigrationContentProperty migrationProperty, SyncMigrationContext context)
    {
        if (string.IsNullOrWhiteSpace(migrationProperty.Value))
        {
            yield break;
        }

        var content = JsonConvert.DeserializeObject<SeoMetadata>(migrationProperty.Value);

        if (content != null)
        {
            yield return new SplitPropertyContent(Properties[nameof(SeoMetadata.Title)].Alias, content.Title);
            yield return new SplitPropertyContent(Properties[nameof(SeoMetadata.Description)].Alias, content.Description);
            yield return new SplitPropertyContent(Properties[nameof(SeoMetadata.NoIndex)].Alias, (content.NoIndex ? 1 : 0).ToString());
            yield return new SplitPropertyContent(Properties[nameof(SeoMetadata.UrlName)].Alias, content.UrlName);
        }
    }

    public IEnumerable<SplitPropertyInfo> GetSplitProperties(string contentTypeAlias, string propertyAlias, string propertyName, SyncMigrationContext context)
    {
        return Properties.Values.Select(x => new SplitPropertyInfo(x.Name, x.Alias, x.EditorAlias, context.DataTypes.GetFirstDefinition(x.OriginalEditorAlias) ?? x.DefaultDefinition));
    }
}

public class SeoMetadata
{
    public string Title { get; set; }
    public string Description { get; set; }
    public bool NoIndex { get; set; }
    public string UrlName { get; set; }
}