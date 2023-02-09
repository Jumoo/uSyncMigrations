using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Grid;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;
using uSync.Migrations.Context;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.BlockGrid.BlockMigrators;
public interface ISyncBlockMigrator
{
	// the grid aliases this block migrator work for., 
	string[] Aliases { get; }

	IEnumerable<NewContentTypeInfo> AdditionalContentTypes(IGridEditorConfig editorConfig);
	IEnumerable<string> GetAllowedContentTypes(IGridEditorConfig config, SyncMigrationContext context);

	string GetContentTypeAlias(GridValue.GridControl control);
	string GetContentTypeAlias(IGridEditorConfig editorConfig);
	string GetEditorAlias(IGridEditorConfig editor);

	Dictionary<string, object> GetPropertyValues(GridValue.GridControl control, SyncMigrationContext context);
}


public class SyncBlockMigratorCollectionBuilder
	: LazyCollectionBuilderBase<SyncBlockMigratorCollectionBuilder, SyncBlockMigratorCollection, ISyncBlockMigrator>
{
	protected override SyncBlockMigratorCollectionBuilder This => this;
}


public class SyncBlockMigratorCollection
	: BuilderCollectionBase<ISyncBlockMigrator>
{
	public SyncBlockMigratorCollection(Func<IEnumerable<ISyncBlockMigrator>> items) : base(items)
	{}

	public ISyncBlockMigrator? GetMigrator(string? gridAlias)
	{
		if (gridAlias == null) return null;
		return this.FirstOrDefault(x => x.Aliases.InvariantContains(gridAlias));
	}

	public ISyncBlockMigrator? GetDefaultMigrator()
		=> GetMigrator("___default___");

	public ISyncBlockMigrator? GetMigrator(GridValue.GridEditor gridEditor)
	{
		var migrator = GetMigrator(gridEditor?.View);
		if (migrator != null) return migrator;

		migrator = GetMigrator(gridEditor?.Alias);
		if (migrator != null) return migrator;

		return GetDefaultMigrator();
	}

	public ISyncBlockMigrator? GetMigrator(IGridEditorConfig editorConfig)
	{
		// we look in a number of places when in a grid editor

		// 1. view
		var migrator = GetMigrator(editorConfig?.View);
		if (migrator != null) return migrator;

		// 2. alias
		migrator = GetMigrator(editorConfig?.Alias);
		if (migrator != null) return migrator;

		// if it goes wrong , we return the default, and everything becomes a label.
		return GetDefaultMigrator();	
	}
}