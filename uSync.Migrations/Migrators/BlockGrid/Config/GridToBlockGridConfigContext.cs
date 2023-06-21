using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

using uSync.Migrations.Legacy.Grid;
using uSync.Migrations.Migrators.BlockGrid.Extensions;

namespace uSync.Migrations.Migrators.BlockGrid.Config;

/// <summary>
///  lets us pass around the context of our conversion of the block grid. 
/// </summary>
/// <remarks>
///  this makes it much easier to block up the code (especially around layout/templates)
///  
///  i think in a perfect world we will be able to refactor this context out of existence. 
/// </remarks>
internal class GridToBlockGridConfigContext
{
    public GridConfiguration GridConfiguration { get; }
    public ILegacyGridEditorsConfig GridEditorsConfig { get; }
    public int? GridColumns { get; }

    public List<BlockGridConfiguration.BlockGridGroupConfiguration> BlockGroups { get; } = new();
    public Dictionary<string, BlockGridConfiguration.BlockGridBlockConfiguration> LayoutBlocks { get; } = new();
    public List<BlockGridConfiguration.BlockGridBlockConfiguration> ContentBlocks { get; } = new();

    public BlockGridConfiguration.BlockGridAreaConfiguration RootArea { get; } = new();
    public Dictionary<BlockGridConfiguration.BlockGridAreaConfiguration, IEnumerable<string>> AllowedEditors { get; } = new();
    public Dictionary<BlockGridConfiguration.BlockGridAreaConfiguration, IEnumerable<string>> AllowedLayouts { get; } = new();

	public BlockGridConfiguration.BlockGridGroupConfiguration LayoutsGroup { get; } = new()
	{
		Key = $"group_{nameof(LayoutsGroup)}".ToGuid(),
		Name = "Layouts"
	};
	public BlockGridConfiguration.BlockGridGroupConfiguration GridBlocksGroup { get; } = new()
	{
		Key = $"group_{nameof(GridBlocksGroup)}".ToGuid(),
		Name = "Grid Blocks"
	};


	public GridToBlockGridConfigContext(GridConfiguration gridConfiguration, ILegacyGridEditorsConfig gridConfig)
    {
        GridEditorsConfig = gridConfig;
        GridConfiguration = gridConfiguration;
        GridColumns = gridConfiguration.GetGridColumns();

        BlockGroups.Add(LayoutsGroup);
        BlockGroups.Add(GridBlocksGroup);
    }

    public IEnumerable<string> GetAllowedLayouts(BlockGridConfiguration.BlockGridAreaConfiguration area)
    {
        if (AllowedLayouts.TryGetValue(area, out var allowed))
            return allowed;

        return Enumerable.Empty<string>();
    }

    public IEnumerable<string> GetRootAllowedLayouts()
        => GetAllowedLayouts(RootArea);

    public void AppendToRootLayouts(IEnumerable<string> allowed)
    {
        var rootAllowed = GetRootAllowedLayouts().ToList();
        rootAllowed.AddRange(allowed);
        AllowedLayouts[RootArea] = rootAllowed;
    }

    public IEnumerable<string> AllEditors()
        => AllowedEditors.SelectMany(x => x.Value).Distinct();

    public BlockGridConfiguration ConvertToBlockGridConfiguration()
    {
        // layoutConfig.ToResult();
        var result = new BlockGridConfiguration();
        result.GridColumns = GridColumns;

        result.Blocks = ContentBlocks
            .Union(LayoutBlocks.Values)
            .Where(x => x.ContentElementTypeKey != Guid.Empty)
            .ToArray();

        var referencedGroupKeys = result.Blocks
            .Select(x => x.GroupKey).Distinct().WhereNotNull()
            .Select(x => Guid.TryParse(x, out var k) ? k : Guid.Empty)
            .Distinct()
            .ToArray();

        result.BlockGroups = BlockGroups
            .Where(x => referencedGroupKeys.Contains(x.Key))
            .ToArray();


        return result;

    }
}
