using HtmlAgilityPack;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Services;

using uSync.Core.Extensions;
using uSync.Core.Mapping;
using uSync.Core.Serialization;

namespace uSync.Migrations.Migrators.Macros;

/// <summary>
///  finds macros in RTE blocks and upgrades them to content blocks, 
///  (assumes the macros have already been upgraded to content types by the MacroFileUpgrader)
/// </summary>
[Weight(200)]
internal class MacroToBlockRteMigrator : SyncValueMapperBase, ISyncMapper
{
    private readonly IContentTypeService _contentTypeService;

    public MacroToBlockRteMigrator(IEntityService entityService, IContentTypeService contentTypeService) : base(entityService)
    {
        _contentTypeService = contentTypeService;
    }

    public override string Name => nameof(MacroToBlockRteMigrator);

    public override string[] Editors => [
        "Umbraco.TinyMCE",
        Constants.PropertyEditors.Aliases.RichText,
        $"{Constants.PropertyEditors.Aliases.Grid}.rte"
    ];

    public override async Task<string?> GetImportValueAsync(string value, string editorAlias, SyncSerializerOptions options)
    {
        // it is the responsiblity of the mapper to know if the import has happened before. 
        if (value.Contains("UMBRACO_MACRO") is false) return value;

        // see if this has been converted to the new block style. 
        if (value.TryDeserialize<RichTextEditorValue>(out var richTextEditorValue) is false || richTextEditorValue is null)
        {
            richTextEditorValue = new RichTextEditorValue
            {
                Markup = value,
                Blocks = new RichTextBlockValue
                {
                    ContentData = [],
                    SettingsData = [],
                }
            };
        }

        var markup = richTextEditorValue.Markup;
        if (markup.Contains("UMBRACO_MACRO") is false) return richTextEditorValue.SerializeJsonString();

        // loads the value as html, finds any comments that contain the macro syntax, parses out the macro alias and parameters,
        // then looks up the content type for the macro and replaces the comment with a content block reference.

        var html = new HtmlDocument();
        html.LoadHtml(markup);

        HtmlNodeCollection? commentNodes = html.DocumentNode.SelectNodes("//comment()[contains(., '<?UMBRACO_MACRO')]");
        if (commentNodes is null || commentNodes.Count == 0) return richTextEditorValue.SerializeJsonString();

        foreach (var commentNode in commentNodes.ToList())
        {
            // parse the macro alias out of the comment, it's in the format <?UMBRACO_MACRO macroAlias param1=value1 param2=value2 ?>
            var node = commentNode.InnerHtml.Replace("<?UMBRACO_MACRO", "<macro").Replace("?>", "/>").Trim();
            var nodeHtml = new HtmlDocument();
            nodeHtml.LoadHtml(node);
            var macroNode = nodeHtml.DocumentNode.SelectSingleNode("//macro");

            var aliasAttribute = macroNode.Attributes.FirstOrDefault(x => x.Name.Equals("macroAlias", StringComparison.InvariantCultureIgnoreCase));
            if (aliasAttribute is null) continue;

            var contentType = await GetMacroContentType(aliasAttribute.Value);
            if (contentType is null) continue;

            var key = Guid.NewGuid();

            var blockItem = new BlockItemData
            {
                Key = key,
                ContentTypeKey = contentType.Key,
            };

            var parameters = new Dictionary<string, string>();
            foreach (var attribute in macroNode.Attributes)
            {
                if (attribute.Name.StartsWith("macroAlias", StringComparison.InvariantCultureIgnoreCase))
                    continue;

                blockItem.Values.Add(new BlockPropertyValue
                {
                    Alias = attribute.Name,
                    Value = attribute.Value
                });
            }

            richTextEditorValue.Blocks?.ContentData.Add(blockItem);

            // replace the block in the html.
            var blockNode = HtmlNode.CreateNode($"<umb-rte-block data-content-key=\"{key}\"></umb-rte-block>");
            commentNode.ParentNode.ReplaceChild(blockNode, commentNode);
        }

        // make the blocklayout, and make the block expose. 
        richTextEditorValue.Markup = html.DocumentNode.OuterHtml;

        // ensure the expose values are also set.
        richTextEditorValue.Blocks?.Expose = GenerateExposeList(richTextEditorValue.Blocks?.ContentData ?? []);
        richTextEditorValue.Blocks?.Layout = GenerateLayout(richTextEditorValue.Blocks?.ContentData ?? []);

        return richTextEditorValue.SerializeJsonString();
    }

    private static IDictionary<string, IEnumerable<IBlockLayoutItem>> GenerateLayout(List<BlockItemData> blockItemDatas)
    {
        return new Dictionary<string, IEnumerable<IBlockLayoutItem>>
        {
            { "Umbraco.RichText",
                blockItemDatas.Select(x => new RichTextBlockLayoutItem
                {
                    ContentKey = x.Key
                })
            }
        };

    }

    private Task<IContentType?> GetMacroContentType(string contentTypeAlias)
        => Task.FromResult(_contentTypeService.Get(contentTypeAlias));

    private List<BlockItemVariation> GenerateExposeList(IEnumerable<BlockItemData> contentData)
        => [.. contentData.Select(x => new BlockItemVariation(x.Key, null, null))];
}
