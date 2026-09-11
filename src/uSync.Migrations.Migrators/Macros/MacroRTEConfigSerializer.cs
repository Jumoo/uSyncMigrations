using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

using uSync.Core.DataTypes;
using uSync.Core.Extensions;
using uSync.Migrations.Core.Migrators;

namespace uSync.Migrations.Migrators.Macros;


/// <summary>
///  adds any generated Macro Content Types to the list of allowed blocks inside an RTE.
/// </summary>
[Weight(200)] // +100, so it happens after the ones in the core usync (which will prep the RTE for tiptap). 
internal class MacroRTEConfigSerializer : SyncConfigurationMigratorBase, IConfigurationSerializer
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IContentTypeContainerService _contentTypeContainerService;

    public MacroRTEConfigSerializer(
        IContentTypeService contentTypeService,
        IContentTypeContainerService contentTypeContainerService)
    {
        _contentTypeService = contentTypeService;
        _contentTypeContainerService = contentTypeContainerService;
    }

    public string Name => nameof(MacroRTEConfigSerializer);

    public string[] Editors => [Constants.PropertyEditors.Aliases.RichText, "Umbraco.TinyMCE"];

    // keep it the same. 
    public override string? TargetEditor => null;

    public override async Task<IDictionary<string, object>> GetMigratedConfigurationAsync(string name, IDictionary<string, object> configuration)
    {
        var richTextBlocks = new List<RichTextConfiguration.RichTextBlockConfiguration>();

        if (configuration.TryGetValue("blocks", out var blocksConfig))
        {
            var blocksJson = blocksConfig.SerializeJsonString();
            if (!string.IsNullOrWhiteSpace(blocksJson))
            {
                richTextBlocks = blocksJson.DeserializeJson<
                    List<RichTextConfiguration.RichTextBlockConfiguration>>() ?? [];
            }
        }

        if (richTextBlocks.Count > 0)
            return configuration;

        var macroContentTypes = await GetMacroContentTypes();
        foreach (var macroContentType in macroContentTypes)
        {
            if (richTextBlocks.Any(x => x.ContentElementTypeKey == macroContentType.Key) is true)
                continue;

            richTextBlocks.Add(new RichTextConfiguration.RichTextBlockConfiguration
            {
                ContentElementTypeKey = macroContentType.Key,
            });
        }

        var updatedConfig = configuration.ToDictionary();
        updatedConfig["blocks"] = richTextBlocks;

        if (richTextBlocks.Count > 0)
        {
            if (updatedConfig["extensions"] is string[] extensionArray)
            {
                List<string> updatedExtensions = [.. extensionArray, "Umb.Tiptap.Block"];
                updatedConfig["extensions"] = updatedExtensions;
            }

            if (updatedConfig["toolbar"] is List<List<List<string>>> toolbar)
            {
                var firstToolbar = toolbar.FirstOrDefault()?.FirstOrDefault();
                if (firstToolbar is not null)
                {
                    if (firstToolbar.Count == 0)
                        firstToolbar.AddRange(_defaultToolbar);

                    firstToolbar.Add("Umb.Tiptap.Toolbar.BlockPicker");
                }
            }
        }

        return updatedConfig;
    }

    private static string[] _defaultToolbar = ["sourcecode", "bold", "italic", "underline", "alignleft", "aligncenter", "alignright",
                          "bullist", "numlist", "outdent", "indent", "link", "umbmediapicker", "umbembeddialog"];

    private async Task<IEnumerable<IContentType>> GetMacroContentTypes()
    {
        List<EntityContainer> macroContainer = [.. await _contentTypeContainerService.GetAsync("Macros", 1)];
        if (macroContainer.Count == 0) return [];
        return _contentTypeService.GetChildren(macroContainer[0].Id);
    }

}
