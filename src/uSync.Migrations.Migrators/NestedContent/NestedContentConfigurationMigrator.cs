using System.Text.Json.Nodes;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

using uSync.Core.DataTypes;
using uSync.Core.Extensions;
using uSync.Migrations.Core.Migrators;
using uSync.Migrations.Core.Tracking;

using static uSync.Migrations.Core.SyncMigrations;

namespace uSync.Migrations.Migrators.NestedContent;


/// <summary>
///  migrates nested content to a block list element. 
/// </summary>

internal class NestedContentConfigurationMigrator : SyncConfigurationMigratorBase, IConfigurationSerializer
{
    private readonly IContentTypeService _contentTypeService;
    private readonly ISyncMigrationTrackingService _trackingService;

    public NestedContentConfigurationMigrator(IContentTypeService contentTypeService, ISyncMigrationTrackingService trackingService)
    {
        _contentTypeService = contentTypeService;
        _trackingService = trackingService;
    }

    public string Name => nameof(NestedContentConfigurationMigrator);
    public string[] Editors => [SyncLegacyTypes.NestedContent, SyncLegacyTypes.OurNestedContent];

    public override string? TargetEditor => Constants.PropertyEditors.Aliases.BlockList;
    public override async Task<IDictionary<string, object>> GetMigratedConfigurationAsync(string name, IDictionary<string, object> configuration)
    {
        var config = new BlockListConfiguration();

        if (configuration.TryGetValue("minItems", out var min) && int.TryParse(min?.ToString(), out var minItems))
            config.ValidationLimit.Min = minItems;

        if (configuration.TryGetValue("maxItems", out var max) && int.TryParse(max?.ToString(), out var maxItems))
            config.ValidationLimit.Max = maxItems;

        if (configuration.TryGetValue("contentTypes", out var contentTypes) && contentTypes is JsonArray contentTypesArray)
        {
            var blocks = new List<BlockListConfiguration.BlockConfiguration>();
            foreach (var contentType in contentTypesArray.OfType<JsonObject>())
            {
                if (contentType["ncAlias"]?.ToString() is not string alias) continue;

                // block list can only use element types, so we track the doc type here
                // (which is before content types are imported) to make it one as it comes in.
                // see NestedContentElementTypeImportingHandler
                await _trackingService.AddRenameAsync(NestedContentElementTypes.GetTrackingKey(alias), alias, name);

                var contentTypeItem = _contentTypeService.Get(alias);
                if (contentTypeItem is null) continue;
                blocks.Add(new BlockListConfiguration.BlockConfiguration
                {
                    ContentElementTypeKey = contentTypeItem.Key,
                });
            }

            config.Blocks = [.. blocks];
        }

        return config.SerializeJsonString().DeserializeJson<Dictionary<string, object>>() ?? configuration;
    }
}
