using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Composing;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Models;

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

    public override object GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var contentTypes = dataTypeProperty.PreValues.GetPreValueOrDefault("contentTypes", "[]");
        var maxItems = dataTypeProperty.PreValues.GetPreValueOrDefault("maxItems", 0);
        var singleItemMode = dataTypeProperty.PreValues.GetPreValueOrDefault("singleItemMode", 0);

        var blocks = JsonConvert.DeserializeObject<List<StackedContentConfigurationBlock>>(contentTypes)?
            .Select(x => new BlockListConfiguration.BlockConfiguration
            {
                ContentElementTypeKey = x.ContentTypeKey,
                Label = x.NameTemplate,
            })
            .ToArray();

        var validationLimit = singleItemMode == 1
             ? new BlockListConfiguration.NumberRange { Min = 1, Max = 1 }
             : new BlockListConfiguration.NumberRange { Min = 0, Max = maxItems };

        return new BlockListConfiguration
        {
            Blocks = blocks ?? Array.Empty<BlockListConfiguration.BlockConfiguration>(),
            ValidationLimit = validationLimit,
        };
    }

    public override string GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (string.IsNullOrWhiteSpace(contentProperty.Value) == false)
        {
            var items = JsonConvert.DeserializeObject<IList<StackedContentItem>>(contentProperty.Value);
            if (items?.Any() == true)
            {
                var contentData = new List<BlockItemData>();
                var layout = new List<BlockListLayoutItem>();

                foreach (var item in items)
                {
                    var contentTypeAlias = context.GetContentTypeAlias(item.ContentTypeKey);

                    foreach (var property in item.Values)
                    {
                        var editorAlias = context.GetEditorAlias(contentTypeAlias, property.Key);
                        if (editorAlias != null)
                        {
                            var migrator = _migrators.Value.Get(editorAlias.OriginalEditorAlias);
                            if (migrator != null)
                            {
                                item.Values[property.Key] = migrator.GetContentValue(new SyncMigrationContentProperty(editorAlias.OriginalEditorAlias, property.Value?.ToString() ?? string.Empty), context);
                            }
                        }
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

                if (contentData.Count > 0)
                {
                    var model = new BlockValue
                    {
                        ContentData = contentData,
                        Layout = new Dictionary<string, JToken>()
                        {
                            { UmbConstants.PropertyEditors.Aliases.BlockList, JArray.FromObject(layout) }
                        },
                    };

                    return JsonConvert.SerializeObject(model, Formatting.Indented);
                }
            }
        }

        return base.GetContentValue(contentProperty, context);
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
