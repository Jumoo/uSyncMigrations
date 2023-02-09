using Umbraco.Cms.Core.Configuration.Grid;
using Umbraco.Cms.Core.Strings;

namespace uSync.Migrations.Migrators.BlockGrid.BlockMigrators;

/// <summary>
///  default migrator, what happens if we can't find anything. 
/// </summary>
public class GridDefaultBlockMigrator : GridBlockMigratorSimpleBase, ISyncBlockMigrator
{
	public GridDefaultBlockMigrator(IShortStringHelper shortStringHelper) : base(shortStringHelper)
	{}

	public string[] Aliases => new[] { "___default___" };
	public override string GetEditorAlias(IGridEditorConfig editor) => "Label (string)";
}


