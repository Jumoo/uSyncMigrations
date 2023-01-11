using Lucene.Net.Codecs;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Polly;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Models;

using static Umbraco.Cms.Core.Constants;

namespace uSync.Migrations.Migrators.Optional;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.NestedContent)]
[SyncMigrator("Our.Umbraco.NestedContent")]
[SyncMigratorVersion(7,8)]
public class NestedToBlockListMigrator : SyncPropertyMigratorBase
{
    public NestedToBlockListMigrator()
    { }

    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.BlockList;

    /// <summary>
    ///  convert a nested datatype config to a block list one.
    /// </summary>
    public override object GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        switch (context.SourceVersion)
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

        var nestedConfig = new NestedContentConfiguration().MapPreValues(dataTypeProperty.PreValues);
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
        var config = new BlockListConfiguration
        {
            ValidationLimit = new BlockListConfiguration.NumberRange
            {
                Max = nestedConfig.MaxItems == 0 ? null : nestedConfig.MaxItems,
                Min = nestedConfig.MinItems == 0 ? null : nestedConfig.MinItems
            },
        };

        if (nestedConfig.ContentTypes != null)
        {
            var elementTypes = new List<Guid>();
            
            var blocks = new List<BlockListConfiguration.BlockConfiguration>();
            foreach (var item in nestedConfig.ContentTypes)
            {
                if (string.IsNullOrWhiteSpace(item.Alias)) continue;

                var contentTypeKey = context.GetContentTypeKey(item.Alias);

                // tell the process we need this to be an element type
                elementTypes.Add(contentTypeKey);

                blocks.Add(new BlockListConfiguration.BlockConfiguration
                {
                    ContentElementTypeKey = contentTypeKey,
                    Label = item.Template
                });
            }
            
            context.AddElementTypes(elementTypes, true);

            config.Blocks = blocks.ToArray();
        }

        return config;
    }


    public override string GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (string.IsNullOrWhiteSpace(contentProperty.Value)) return string.Empty;
        var rowValues = JsonConvert.DeserializeObject<IList<NestedContentRowValue>>(contentProperty.Value);
        if (rowValues == null) return string.Empty;

        var blockValue = new BlockValue();
        var layoutItems = new List<BlockListLayoutItem>();

        foreach (var row in rowValues)
        {
            var contentTypeKey = context.GetContentTypeKey(row.ContentTypeAlias);

            var block = new BlockItemData
            {
                Udi = Udi.Create(UdiEntityType.Element, row.Id),
                ContentTypeKey = contentTypeKey,
                ContentTypeAlias = row.ContentTypeAlias,
            };

            layoutItems.Add(new BlockListLayoutItem
            {
                ContentUdi = block.Udi,
            });

            foreach (var (propertyAlias, propertyValue) in row.RawPropertyValues)
            {
                if (propertyValue == null)
                {
                    continue;
                }
                
                var editorAlias = context.GetEditorAlias(row.ContentTypeAlias, propertyAlias);
                if (editorAlias == null) continue;

                var migrator = context.TryGetMigrator(editorAlias.OriginalEditorAlias);
                if (migrator != null)
                {
                    var value = propertyValue?.ToString() ?? string.Empty;
                    var property = new SyncMigrationContentProperty(row.ContentTypeAlias, value);
                    var migratedValue = migrator.GetContentValue(property, context);
                    block.RawPropertyValues[propertyAlias] = migratedValue;
                }
            }

            blockValue.ContentData.Add(block);
        }

        blockValue.Layout = new Dictionary<string, JToken>
        {
            { PropertyEditors.Aliases.BlockList, JArray.FromObject(layoutItems) },
        };

        return JsonConvert.SerializeObject(blockValue, Formatting.Indented);
    }
}
