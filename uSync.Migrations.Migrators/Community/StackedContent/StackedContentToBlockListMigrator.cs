using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;
using uSync.Migrations.Core.Extensions;

namespace uSync.Migrations.Migrators.Community.StackedContent;

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
        var maxItems = dataTypeProperty.PreValues?.GetPreValueOrDefault("maxItems", default(int?)) ?? default(int?);
        maxItems = maxItems == 0 ? null : maxItems;
        var singleItemMode = dataTypeProperty.PreValues?.GetPreValueOrDefault("singleItemMode", 0) ?? 0;

        var blocks = JsonConvert
            .DeserializeObject<List<StackedContentConfigurationBlock>>(contentTypes)?
            .Select(x => new BlockListConfiguration.BlockConfiguration
            {
                ContentElementTypeKey = GetElementKey(x),
                Label = x.NameTemplate,
            })
            .Where(x => x.ContentElementTypeKey != Guid.Empty)
            .ToArray() ?? Array.Empty<BlockListConfiguration.BlockConfiguration>();  

        if (blocks.Length > 0)
        {
            context.ContentTypes.AddElementTypes(blocks.Select(x => x.ContentElementTypeKey), true);
        }

        var validationLimit = singleItemMode == 1
             ? new BlockListConfiguration.NumberRange { Min = 1, Max = 1 }
             : new BlockListConfiguration.NumberRange { Min = 0, Max = maxItems };

        return new BlockListConfiguration
        {
            Blocks = blocks,
            ValidationLimit = validationLimit
        };

        Guid GetElementKey(StackedContentConfigurationBlock configurationBlock)
        {
            if (string.IsNullOrWhiteSpace(configurationBlock.ContentTypeAlias) is true) return configurationBlock.ContentTypeKey;
            if (context.ContentTypes.TryGetKeyByAlias(configurationBlock.ContentTypeAlias, out var key) is true) return key;
            return configurationBlock.ContentTypeKey;
        }
    }

    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (string.IsNullOrWhiteSpace(contentProperty.Value))
        {
            return string.Empty;
        }

        var items = JsonConvert.DeserializeObject<IList<StackedContentItem>>(contentProperty.Value, new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None });
        if (items?.Any() != true)
        {
            return string.Empty;
        }

        var contentData = new List<BlockItemData>();

        var layout = new List<BlockListLayoutItem>();

        foreach (var item in items)
        {
            var contentTypeKey = item.ContentTypeKey;
            var contentTypeAlias = item.ContentTypeAlias;
            if (string.IsNullOrWhiteSpace(item.ContentTypeAlias) is false)
            {
                if (context.ContentTypes.TryGetAliasByKey(item.ContentTypeKey, out contentTypeAlias) is false)
                {
                    contentTypeAlias = item.ContentTypeAlias;
                }
            }
            else
            {
                if (context.ContentTypes.TryGetKeyByAlias(item.ContentTypeAlias, out contentTypeKey) is false)
                {
                    contentTypeKey = item.ContentTypeKey;
                }
            }

            foreach (var (propertyAlias, value) in item.Values)
            {
                // var editorAlias = context.ContentTypes.GetEditorAliasByTypeAndProperty(contentTypeAlias, propertyAlias);

                if (context.ContentTypes.TryGetEditorAliasByTypeAndProperty(contentTypeAlias, propertyAlias, out var editorAlias) is false) { continue; }
                if (context.Migrators.TryGetMigrator(editorAlias.OriginalEditorAlias, out var migrator) is false) { continue; }               

                var childProperty = new SyncMigrationContentProperty(editorAlias.OriginalEditorAlias,
                    contentTypeAlias, propertyAlias,
                    value?.ToString() ?? string.Empty);

                item.Values[propertyAlias] = migrator.GetContentValue(childProperty, context);
            }

            var block = new BlockItemData
            {
                ContentTypeKey = contentTypeKey,
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
        [JsonProperty("icContentTypeAlias")]
        public string ContentTypeAlias { get; set; }
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
        [JsonProperty("icContentTypeAlias")]
        public string ContentTypeAlias { get; set; }

        [JsonProperty("nameTemplate")]
        public string? NameTemplate { get; set; }
    }
}
