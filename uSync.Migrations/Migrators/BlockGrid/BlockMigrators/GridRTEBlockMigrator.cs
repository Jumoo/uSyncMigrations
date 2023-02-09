using Umbraco.Cms.Core.Configuration.Grid;
using Umbraco.Cms.Core.Strings;

namespace uSync.Migrations.Migrators.BlockGrid.BlockMigrators;

public class GridRTEBlockMigrator : GridBlockMigratorSimpleBase, ISyncBlockMigrator
{

	public GridRTEBlockMigrator(IShortStringHelper shortStringHelper)
		: base(shortStringHelper) { }

	public string[] Aliases => new[] { "rte" };

	public override string GetEditorAlias(IGridEditorConfig editor) => "Richtext Editor";
}


