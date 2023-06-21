using Umbraco.Cms.Core.Strings;

using uSync.Migrations.Legacy.Grid;

namespace uSync.Migrations.Migrators.BlockGrid.BlockMigrators;

public class GridRTEBlockMigrator : GridBlockMigratorSimpleBase, ISyncBlockMigrator
{

	public GridRTEBlockMigrator(IShortStringHelper shortStringHelper)
		: base(shortStringHelper) { }

	public string[] Aliases => new[] { "rte" };

	public override string GetEditorAlias(ILegacyGridEditorConfig editor) => "Richtext Editor";
}


