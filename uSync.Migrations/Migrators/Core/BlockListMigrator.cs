using Newtonsoft.Json;
using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Context;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.BlockList, typeof(BlockListConfiguration), IsDefaultAlias = true)]
[SyncDefaultMigrator]
[SyncMigratorVersion(8)]
public class BlockListMigrator : SyncPropertyMigratorBase
{
    public BlockListMigrator()
    { }

    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.BlockList;

    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        if (string.IsNullOrWhiteSpace(dataTypeProperty.ConfigAsString)) return new BlockListConfiguration();

        var config = JsonConvert.DeserializeObject<BlockListConfiguration>(dataTypeProperty.ConfigAsString);

        if (config == null) return new BlockListConfiguration();

        foreach(var contentTypeKey in config.Blocks.Select(x => x.ContentElementTypeKey))
        {
            if (contentTypeKey == null) continue;

            context.ContentTypes.AddElementType(contentTypeKey);
        }

        foreach (var settingsTypeKey in config.Blocks.Select(x => x.SettingsElementTypeKey))
        {
            if (settingsTypeKey == null) continue;

            context.ContentTypes.AddElementType(settingsTypeKey.Value);
        }

        return config;
    }

    // TODO: [KJ] Nested content GetContentValue (so we can recurse)
    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (string.IsNullOrWhiteSpace(contentProperty.Value)) return string.Empty;

        var blockList = JsonConvert.DeserializeObject<BlockListValue>(contentProperty.Value);

        if (blockList == null) return string.Empty;

        foreach (var row in blockList.ContentData)
        {
            MigratePropertiesWithin(context, row);
        }

        foreach (var row in blockList.SettingsData)
        {
            MigratePropertiesWithin(context, row);
        }

        return JsonConvert.SerializeObject(blockList, Formatting.Indented);
    }

    private static void MigratePropertiesWithin(SyncMigrationContext context, BlockListRowValue row)
    {
        foreach (var property in row.RawPropertyValues)
        {
            var contentTypeAlias = context.ContentTypes.GetAliasByKey(row.ContentTypeKey);

            var editorAlias = context.ContentTypes.GetEditorAliasByTypeAndProperty(contentTypeAlias, property.Key);
            if (editorAlias == null) continue;

            var migrator = context.Migrators.TryGetMigrator(editorAlias.OriginalEditorAlias);
            if (migrator != null)
            {
                row.RawPropertyValues[property.Key] = migrator.GetContentValue(
                    new SyncMigrationContentProperty(
                        contentTypeAlias, property.Key, contentTypeAlias, property.Value?.ToString()),
                        context);
            }
        }
    }

    internal class BlockListValue
    {
        [JsonProperty("layout")]
        public BlockListLayoutValue? Layout { get; set; }

        [JsonProperty("contentData")]
        public BlockListRowValue[]? ContentData { get; set; }

        [JsonProperty("settingsData")]
        public BlockListRowValue[]? SettingsData { get; set; } = null!;
    }

    internal class BlockListLayoutValue
    {
        [JsonProperty("Umbraco.BlockList")]
        public BlockUdiValue[]? BlockOrder { get; set; }
    }

    internal class BlockUdiValue
    {
        [JsonProperty("contentUdi")]
        public string? ContentUdi { get; set; }

        [JsonProperty("settingsUdi")]
        public string? SettingsUdi { get; set; }
    }

    internal class BlockListRowValue
    {
        [JsonProperty("contentTypeKey")]
        public Guid ContentTypeKey { get; set; }

        [JsonProperty("udi")]
        public string? Udi { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object?> RawPropertyValues { get; set; } = null!;
    }
}

