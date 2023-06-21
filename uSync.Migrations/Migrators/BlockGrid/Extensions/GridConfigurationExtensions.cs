using Newtonsoft.Json.Linq;

using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

using uSync.Migrations.Context;
using uSync.Migrations.Legacy.Grid;
using uSync.Migrations.Migrators.BlockGrid.BlockMigrators;
using uSync.Migrations.Migrators.BlockGrid.Models;

namespace uSync.Migrations.Migrators.BlockGrid.Extensions;
internal static class GridConfigurationExtensions
{
    public static int? GetGridColumns(this GridConfiguration gridConfiguration)
    {
        if (gridConfiguration.Items?.TryGetValue("columns", out var columns) == true)
        {
            return columns.Value<int>();
        }

        return 12;
    }

    public static JToken? GetItemBlock(this GridConfiguration gridConfiguration, string name)
    {
        if (gridConfiguration.Items?.TryGetValue(name, out var block) == true)
        {
            return block;
        }

        return null;
    }

    /// <summary>
    ///  returns all the allowed content type aliases for a given grid editor config block
    /// </summary>
    public static IEnumerable<string> GetAllowedContentTypeAliasesForBlock(this ILegacyGridEditorConfig editorConfig, SyncMigrationContext context, SyncBlockMigratorCollection blockMigrators)
    {
        // mainly a doctypegrid thing, but also for generic text, rtes

        var blockMigrator = blockMigrators.GetMigrator(editorConfig);
        if (blockMigrator == null) return Enumerable.Empty<string>();
        return blockMigrator.GetAllowedContentTypes(editorConfig, context);
    }

    /// <summary>
    ///  Converts a GriddEditorConfig into a BlockGridBlock 
    /// </summary>
    public static IEnumerable<BlockGridConfiguration.BlockGridBlockConfiguration> ConvertToBlockGridBlocks(this ILegacyGridEditorConfig editorConfig, SyncMigrationContext context, SyncBlockMigratorCollection blockMigrators, Guid groupKey)
    {
        foreach (var allowedAlias in editorConfig.GetAllowedContentTypeAliasesForBlock(context, blockMigrators))
        {
            var elementKey = context.ContentTypes.GetKeyByAlias(allowedAlias);

            yield return new BlockGridConfiguration.BlockGridBlockConfiguration
            {
                Label = editorConfig.GetBlockname(),
                ContentElementTypeKey = elementKey,
                GroupKey = groupKey != Guid.Empty ? groupKey.ToString() : null,
                BackgroundColor = Grid.GridBlocks.Background,
                IconColor = Grid.GridBlocks.Icon
            };
        }
    }

    /// <summary>
    ///  return the name name for a block based on the editor config. 
    /// </summary>
    /// <param name="editorConfig"></param>
    /// <returns></returns>
    public static string GetBlockname(this ILegacyGridEditorConfig? editorConfig)
    {
        if (editorConfig?.Config.TryGetValue("nameTemplate", out var nameTemplateValue) == true)
        {
            return nameTemplateValue as string ?? editorConfig?.Name ?? string.Empty;
        }

        // 
        return editorConfig?.Name ?? string.Empty;
    }
}
