using Umbraco.Cms.Core.Strings;

using uSync.Migrations.Legacy.Grid;

namespace uSync.Migrations.Migrators.BlockGrid.BlockMigrators;

/// <summary>
///  these are the 'simple' grid migrators, they make some assuptions (like a textstring datatype exists)
///  but they allow us to get the basics of what a grid element will look like when we are in the block grid.
/// </summary>

public class GridTextstringBlockMigrator : GridBlockMigratorSimpleBase, ISyncBlockMigrator
{
	public GridTextstringBlockMigrator(IShortStringHelper shortStringHelper)
		: base(shortStringHelper)
	{ }

	public string[] Aliases => new[] { "textstring" };

	public override string GetEditorAlias(ILegacyGridEditorConfig editor) => "Textstring";
}


