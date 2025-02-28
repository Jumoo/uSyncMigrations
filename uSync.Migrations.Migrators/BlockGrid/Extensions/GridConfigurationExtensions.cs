using System.Drawing.Text;
using System.Text.RegularExpressions;

using Newtonsoft.Json.Linq;

using Org.BouncyCastle.Asn1.Cms;

using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

using uSync.Migrations.Core.Legacy.Grid;
using uSync.Migrations.Migrators.BlockGrid.BlockMigrators;
using uSync.Migrations.Migrators.BlockGrid.Models;

using GridConfiguration = Umbraco.Cms.Core.PropertyEditors.GridConfiguration;

namespace uSync.Migrations.Migrators.BlockGrid.Extensions;
internal static class GridConfigurationExtensions
{
    public static int? GetGridColumns(this LegacyGridConfiguration gridConfiguration)
    {
        if (gridConfiguration.Items?.TryGetValue("columns", out var columns) == true)
        {
            return columns.Value<int>();
        }

        return 12;
    }

    public static JToken? GetItemBlock(this LegacyGridConfiguration gridConfiguration, string name)
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
            var keys = new List<Guid>();
            if (Regex.IsMatch(allowedAlias, "\\W"))
            {
                var matchingAliases = context.ContentTypes.GetAllAliases().Where(x => Regex.IsMatch(x, allowedAlias)).ToList();

                keys.AddRange(matchingAliases.Select(x =>
                {
                    if (context.ContentTypes.TryGetKeyByAlias(x, out var key))
                        return key;

                    return Guid.Empty;
                }).Where(x => x != Guid.Empty)); 
            }
            else
            {
                if (context.ContentTypes.TryGetKeyByAlias(allowedAlias, out var allowedKey))
                    keys.Add(allowedKey);
            }

            foreach (var elementKey in keys)
            {
                if (context.ContentTypes.TryGetAliasByKey(elementKey, out var contentTypeAlias) is false) { continue; }    

                var label = editorConfig.GetBlockname();
                /*if (keys.Count > 0)
                {
                    label = $"{label} ({context.ContentTypes.GetAliasByKey(elementKey)})";
                }*/
                yield return new BlockGridConfiguration.BlockGridBlockConfiguration
                {
                    Label = label,
                    ContentElementTypeKey = elementKey,
                    GroupKey = groupKey != Guid.Empty ? groupKey.ToString() : null,
                    BackgroundColor = Grid.GridBlocks.Background,
                    IconColor = Grid.GridBlocks.Icon,
                    View = Grid.GridBlocks.View,
                    AllowAtRoot = false
                };
            }
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
            if (!string.IsNullOrEmpty(nameTemplateValue as string))
            {
                return (nameTemplateValue as string)!;
            }
        }

        // 
        return editorConfig?.Name ?? string.Empty;
    }
}
