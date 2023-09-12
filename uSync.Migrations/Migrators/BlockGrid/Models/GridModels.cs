using Newtonsoft.Json;

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



internal class GridTemplateConfiguration
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("sections")]
    public IEnumerable<GridSectionConfiguration>? Sections { get; set; }
}

internal class GridSectionConfiguration
{
    [JsonProperty("grid")]
    public int Grid { get; set; }

    [JsonProperty("allowAll")]
    public bool? AllowAll { get; set; }

    [JsonProperty("allowed")]
    public string[]? Allowed { get; set; }
}

internal class GridLayoutConfiguration
{
    [JsonProperty("label")]
    public string? Label { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("areas")]
    public IEnumerable<GridAreaConfiguration>? Areas { get; set; }
}

internal class GridAreaConfiguration
{
    [JsonProperty("grid")]
    public int Grid { get; set; }

    [JsonProperty("allowAll")]
    public bool? AllowAll { get; set; }

    [JsonProperty("allowed")]
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