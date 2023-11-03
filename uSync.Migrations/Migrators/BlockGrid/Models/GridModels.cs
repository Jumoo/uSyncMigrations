using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Umbraco.Cms.Core.Models.Blocks;

namespace uSync.Migrations.Migrators.BlockGrid.Models;

internal static class Grid 
{ 
    internal static class LayoutBlocks
    {
        public const string Background = "#cfe2f3";
        public const string Icon = "#2986cc";
	}

    internal static class GridBlocks
    {
        public const string Background = "#fce5cd";
        public const string Icon = "#ce7e00"; 
    }
}



[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
internal class GridTemplateConfiguration
{
    public string? Name { get; set; }

    public IEnumerable<GridSectionConfiguration>? Sections { get; set; }
}

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
internal class GridSectionConfiguration
{
    public int Grid { get; set; }
    public bool? AllowAll { get; set; }
    public string[]? Allowed { get; set; }
}

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
internal class GridLayoutConfiguration
{
    public string? Label { get; set; }

    public string? Name { get; set; }

    public IEnumerable<GridAreaConfiguration>? Areas { get; set; }
}

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
internal class GridAreaConfiguration
{
    public int Grid { get; set; }
    public bool? AllowAll { get; set; }
    public string[]? Allowed { get; set; }
}
internal class GridSettingsConfiguration
{
    GridSettingsConfigurationItem[]? ConfigItems { get; set; }
}

internal class GridSettingsConfigurationItem
{
    [JsonProperty("label")]
    public string? Label { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("key")]
    public string? Key { get; set; }

    [JsonProperty("view")]
    public string? View { get; set; }

    [JsonProperty("modifier")]
    public string? Modifier { get; set; }

    [JsonProperty("applyTo")]
    public string? ApplyTo { get; set; }
}
/// <summary>
///  contains the data for a block (content and settings)
/// </summary>
internal class BlockContentPair
{
    public BlockItemData Content { get; set; }
    public BlockItemData? Settings { get; set; }

    public BlockContentPair(BlockItemData content, BlockItemData? settings)
    {
        Content = content;
        Settings = settings;
    }
}