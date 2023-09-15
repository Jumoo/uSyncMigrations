using System.Text.Json;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace uSync.Migrations.Migrators.BlockGrid.Extensions;

internal class GridConventions
{
    public IShortStringHelper ShortStringHelper { get; }

    public GridConventions(IShortStringHelper shortStringHelper)
    {
        ShortStringHelper = shortStringHelper;
    }

    public string AreaAlias(int index)
        => $"area_{index}";

    public string SectionContentTypeAlias(string? name)
        => $"section_{name}".GetBlockGridLayoutContentTypeAlias(ShortStringHelper);

    public string RowLayoutContentTypeAlias(string? name)
        => $"{name}".GetBlockElementContentTypeAlias(ShortStringHelper);

    public string GridAreaConfigAlias(string areaAlias)
        => areaAlias.GetBlockGridAreaConfigurationAlias(ShortStringHelper);

    public string TemplateContentTypeAlias(string template)
        => template.GetBlockElementContentTypeAlias(ShortStringHelper);

    public string LayoutAreaAlias(string layout, string areaAlias)
        => $"layout_{layout}_{areaAlias}".GetBlockGridAreaConfigurationAlias(ShortStringHelper);

    public string LayoutContentTypeAlias(string layout)
        => layout.GetBlockGridLayoutContentTypeAlias(ShortStringHelper);

    public string LayoutSettingsContentTypeAlias(string layout)
        => layout.GetBlockGridLayoutSettingsContentTypeAlias(ShortStringHelper);

    public string FormatGridSettingKey(string setting)
    {
        var splitString = setting.Split("-");
        if (splitString.Length == 1)
        {
            return splitString[0];
        }

        return string.Join("", 
            splitString.First().ToLower(), 
            string.Join("", splitString.Skip(1).Select(s => s.ToFirstUpper())))
            .ToString();
    }
}
