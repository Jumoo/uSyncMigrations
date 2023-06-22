using Umbraco.Cms.Core.Configuration.Grid;

namespace uSync.Migrations.Legacy.Grid;
internal static class LegacyGridExtensions
{
    public static ILegacyGridEditorsConfig ToLegacyEditorsConfig(this IGridEditorsConfig editorsConfig)
    {
        return new LegacyGridEditorsConfig
        {
            Editors = editorsConfig.Editors.Select(x => x.ToLegacyEditorConfig()).ToList()
        };
    }

    public static ILegacyGridEditorConfig ToLegacyEditorConfig(this IGridEditorConfig gridEditorConfig)
        => new LegacyGridEditorConfig
        {
            Alias = gridEditorConfig.Alias,
            Config = gridEditorConfig.Config,
            Icon = gridEditorConfig.Icon,
            Name = gridEditorConfig.Name,
            NameTemplate = gridEditorConfig.NameTemplate,
            Render = gridEditorConfig.Render,
            View = gridEditorConfig.View
        };
    
}
