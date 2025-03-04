using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

using uSync.Migrations.Core.Extensions;
using uSync.Migrations.Migrators.Core;

using static Umbraco.Cms.Core.Constants;

namespace uSync.Migrations.Migrators.Optional;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.NestedContent)]
[SyncMigrator("Our.Umbraco.NestedContent")]
[SyncMigratorVersion(7, 8)]
public class NestedToBlockListMigrator : SyncPropertyMigratorBase
{
    private readonly ILogger<NestedToBlockListMigrator> _logger;

    public NestedToBlockListMigrator(ILogger<NestedToBlockListMigrator> logger)
    {
        _logger = logger;
    }

    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.BlockList;

    /// <summary>
    ///  convert a nested datatype config to a block list one.
    /// </summary>
    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        switch (context.Metadata.SourceVersion)
        {
            case 7:
                return GetVersionSevenConfigValues(dataTypeProperty, context);
            case 8:
                return GetVersionEightConfigValues(dataTypeProperty, context);
        }

        return new BlockListConfiguration();
    }

    private object GetVersionSevenConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        if (dataTypeProperty.PreValues == null)
            return new BlockListConfiguration();

        var nestedConfig = (NestedContentConfiguration?)(new NestedContentConfiguration().MapPreValues(dataTypeProperty.PreValues));
        if (nestedConfig == null) return new BlockListConfiguration();

        return GetBlockListConfigFromNestedConfig(nestedConfig, context);

    }

    private object GetVersionEightConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        if (string.IsNullOrWhiteSpace(dataTypeProperty.ConfigAsString))
            return new BlockListConfiguration();

        var nestedConfig = JsonConvert.DeserializeObject<NestedContentConfiguration>(dataTypeProperty.ConfigAsString);
        if (nestedConfig == null) return new BlockListConfiguration();

        return GetBlockListConfigFromNestedConfig(nestedConfig, context);
    }

    private object GetBlockListConfigFromNestedConfig(NestedContentConfiguration nestedConfig, SyncMigrationContext context)
    {
        var config = new BlockListConfiguration()
        {
            ValidationLimit = new BlockListConfiguration.NumberRange
            {
                Max = nestedConfig.MaxItems == 0 ? null : nestedConfig.MaxItems,
                Min = nestedConfig.MinItems == 0 ? null : nestedConfig.MinItems
            },
            MaxPropertyWidth = "100%",
            UseInlineEditingAsDefault = true
        };

        if (nestedConfig.ContentTypes != null)
        {
            var blocks = new List<BlockListConfiguration.BlockConfiguration>();
            foreach (var item in nestedConfig.ContentTypes)
            {
                if (string.IsNullOrWhiteSpace(item.Alias)) continue;

                var alias = context.ContentTypes.GetReplacementAlias(item.Alias);

                var contentTypeKey = context.ContentTypes.GetKeyByAlias(alias);

                // tell the process we need this to be an element type
                context.ContentTypes.AddElementTypes(new[] { contentTypeKey }, true);

                blocks.Add(new BlockListConfiguration.BlockConfiguration
                {
                    ContentElementTypeKey = contentTypeKey,
                    Label = item.Template
                });
            }

            config.Blocks = blocks.ToArray();
        }

        return config;
    }

    /// <summary>
    ///  stub of what block list json looks like, 
    /// </summary>
    /// <remarks>
    ///  if a property has already been converted, then it will have this json in it. 
    /// </remarks>
    private static string blockListJsonStub = "{\r\n  \"layout\": {\r\n    \"Umbraco.BlockList\":";

    /// <summary>
    ///  convert the content value from nested content to blocklist. 
    /// </summary>
    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (string.IsNullOrWhiteSpace(contentProperty.Value)) return string.Empty;

        if (contentProperty.Value.InvariantStartsWith(blockListJsonStub))
        {
            _logger.LogDebug("Property [{name}] is already BlockList", contentProperty.EditorAlias);
            return contentProperty.Value;
        }

        var rowValues = JsonConvert.DeserializeObject<IList<NestedContentRowValue>>(contentProperty.Value, new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None });
        if (rowValues == null) return string.Empty;

        var blockValue = new BlockValue();
        var contentData = new List<BlockItemData>();
        var blockListLayout = new List<BlockListLayoutItem>();

        foreach (var row in rowValues)
        {
            var contentTypeAlias = context.ContentTypes.GetReplacementAlias(row.ContentTypeAlias);
            if (row.Id == Guid.Empty)
                row.Id = $"{contentProperty.EditorAlias}{contentProperty.ContentTypeAlias}{contentTypeAlias}{row.Name}".ToGuid();

            var contentTypeKey = context.ContentTypes.GetKeyByAlias(contentTypeAlias);
            var blockUdi = Udi.Create(UdiEntityType.Element, row.Id);

            var block = new BlockItemData
            {
                ContentTypeKey = contentTypeKey,
                Udi = blockUdi
            };

            blockListLayout.Add(new BlockListLayoutItem
            {
                ContentUdi = blockUdi,
            });

            foreach (var property in row.RawPropertyValues)
            {
                _logger.LogDebug("NestedToBlockList: {ContentType} {key}", contentTypeAlias, property.Key);

                var editorAlias = context.ContentTypes.GetEditorAliasByTypeAndProperty(contentTypeAlias, property.Key);
                if (editorAlias == null) continue;

                _logger.LogDebug("NestedToBlockList: Property: {editorAlias}", editorAlias);

                var migrator = context.Migrators.TryGetMigrator(editorAlias.OriginalEditorAlias);
                if (migrator != null)
                {
                    _logger.LogDebug("NestedToBlockList: Found Migrator: {migrator}", migrator.GetType().Name);

                    block.RawPropertyValues[property.Key] = migrator.GetContentValue(
                        new SyncMigrationContentProperty(
                            contentTypeAlias,
                            property.Key,
                            editorAlias.OriginalEditorAlias, property.Value?.ToString() ?? string.Empty), context);
                }
                else
                {
                    _logger.LogDebug("NestedToBlockList: No Migrator found");
                    block.RawPropertyValues[property.Key] = property.Value;
                }
            }

            contentData.Add(block);
        }

        blockValue.ContentData = contentData;
        blockValue.Layout = new Dictionary<string, JToken>()
        {
            { "Umbraco.BlockList", JToken.FromObject(blockListLayout) }
        };

        return JsonConvert.SerializeObject(blockValue, Formatting.Indented);
    }
}
