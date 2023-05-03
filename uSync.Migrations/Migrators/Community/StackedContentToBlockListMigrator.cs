using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;
using uSync.Migrations.Composing;
using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators;

[SyncMigrator("Our.Umbraco.StackedContent")]
public class StackedContentToBlockListMigrator : SyncPropertyMigratorBase
{
    Lazy<SyncPropertyMigratorCollection> _migrators;

    public StackedContentToBlockListMigrator(Lazy<SyncPropertyMigratorCollection> migrators)
    {
        _migrators = migrators;
    }

    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.BlockList;

    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var contentTypes = dataTypeProperty.PreValues?.GetPreValueOrDefault("contentTypes", "[]") ?? "[]";
        var maxItems = dataTypeProperty.PreValues?.GetPreValueOrDefault("maxItems", 0) ?? 0;
        var singleItemMode = dataTypeProperty.PreValues?.GetPreValueOrDefault("singleItemMode", 0) ?? 0;

        var blocks = JsonConvert
            .DeserializeObject<List<StackedContentConfigurationBlock>>(contentTypes)?
            .Select(x => new BlockListConfiguration.BlockConfiguration
            {
                ContentElementTypeKey = x.ContentTypeKey,
                Label = x.NameTemplate,
            })
            .ToArray();

        if (blocks?.Any() == true)
        {
            foreach (var elementTypeKey in blocks.Select(x => x.ContentElementTypeKey))
            {
                context.ContentTypes.AddElementType(elementTypeKey);
            }
        }

        var validationLimit = singleItemMode == 1
             ? new BlockListConfiguration.NumberRange { Min = 1, Max = 1 }
             : new BlockListConfiguration.NumberRange { Min = 0, Max = maxItems };

        return new BlockListConfiguration
        {
            Blocks = blocks ?? Array.Empty<BlockListConfiguration.BlockConfiguration>(),
            ValidationLimit = validationLimit,
        };
    }

    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (string.IsNullOrWhiteSpace(contentProperty.Value))
        {
            return string.Empty;
        }

        var items = JsonConvert.DeserializeObject<IList<StackedContentItem>>(contentProperty.Value);
        if (items?.Any() != true)
        {
            return string.Empty;
        }

        var contentData = new List<BlockItemData>();

        var layout = new List<BlockListLayoutItem>();

        foreach (var item in items)
        {
            var contentTypeAlias = context.ContentTypes.GetAliasByKey(item.ContentTypeKey);

            foreach (var (propertyAlias, value) in item.Values)
            {
                var editorAlias = context.ContentTypes.GetEditorAliasByTypeAndProperty(contentTypeAlias, propertyAlias);

                if (editorAlias == null)
                {
                    continue;
                }

                var migrator = _migrators.Value
                    .FirstOrDefault(x => x.Editors.InvariantContains(editorAlias.OriginalEditorAlias));

                if (migrator == null)
                {
                    continue;
                }

                var childProperty = new SyncMigrationContentProperty(editorAlias.OriginalEditorAlias,
                    contentTypeAlias, propertyAlias,
                    value?.ToString() ?? string.Empty);

                item.Values[propertyAlias] = migrator.GetContentValue(childProperty, context);
            }

            var block = new BlockItemData
            {
                ContentTypeKey = item.ContentTypeKey,
                Udi = Udi.Create(UmbConstants.UdiEntityType.Element, item.Key),
                RawPropertyValues = item.Values,
            };

            layout.Add(new BlockListLayoutItem { ContentUdi = block.Udi });

            contentData.Add(block);
        }

        if (contentData.Any() == false)
        {
            return string.Empty;
        }

        var model = new BlockValue
        {
            ContentData = contentData,
            Layout = new Dictionary<string, JToken>
            {
                { UmbConstants.PropertyEditors.Aliases.BlockList, JArray.FromObject(layout) },
            },
        };

        return JsonConvert.SerializeObject(model, Formatting.Indented);
    }

    internal class StackedContentItem
    {
        [JsonProperty("icContentTypeGuid")]
        public Guid ContentTypeKey { get; set; }

        [JsonProperty("key")]
        public Guid Key { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("icon")]
        public string? Icon { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object?> Values { get; set; } = null!;
    }

    internal class StackedContentConfigurationBlock
    {
        [JsonProperty("icContentTypeGuid")]
        public Guid ContentTypeKey { get; set; }

        [JsonProperty("nameTemplate")]
        public string? NameTemplate { get; set; }
    }
}
