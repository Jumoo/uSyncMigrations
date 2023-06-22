using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;
using uSync.Migrations.Context;
using uSync.Migrations.Legacy.Grid;
using uSync.Migrations.Migrators.BlockGrid.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.BlockGrid.BlockMigrators;

/// <summary>
///  base for 'simple' grid things (like text, quote, etc) they all follow a pattern.
/// </summary>
public abstract class GridBlockMigratorSimpleBase
{
	protected readonly IShortStringHelper _shortStringHelper;

	public GridBlockMigratorSimpleBase(IShortStringHelper shortStringHelper)
	{
		_shortStringHelper = shortStringHelper;
	}

	public abstract string GetEditorAlias(ILegacyGridEditorConfig editor);

	/// <summary>
	///  for the simple migrators we create a new doctype to try and migrate the data into.
	/// </summary>
	/// <param name="editorConfig"></param>
	/// <returns></returns>
	public IEnumerable<NewContentTypeInfo> AdditionalContentTypes(ILegacyGridEditorConfig editor)
	{
		var alias = this.GetContentTypeAlias(editor);

		return new NewContentTypeInfo
		{
			Key = alias.ToGuid(),
			Alias = alias,
			Name = editor.Name ?? editor.Alias,
			Description = $"Converted from Grid {editor.Name} element",
			Icon = $"{editor.Icon ?? "icon-book"} color-purple",
			IsElement = true,
			Folder = "BlockGrid/Elements",
			Properties = new List<NewContentTypeProperty>
			{
				new NewContentTypeProperty
				{
					Alias = editor.Alias,
					Name = editor.Name ?? editor.Alias,
					DataTypeAlias = this.GetEditorAlias(editor)
				}
			}
		}.AsEnumerableOfOne();
	}

	public virtual IEnumerable<string> GetAllowedContentTypes(ILegacyGridEditorConfig config, SyncMigrationContext context)
		=> GetContentTypeAlias(config).AsEnumerableOfOne();

	public virtual string GetContentTypeAlias(GridValue.GridControl control)
		=> control.Editor.Alias.GetBlockElementContentTypeAlias(_shortStringHelper);

	public virtual string GetContentTypeAlias(ILegacyGridEditorConfig editorConfig)
	{
		var alias = string.IsNullOrEmpty(editorConfig.Alias) 
			? Path.GetFileNameWithoutExtension(editorConfig.View)
			: editorConfig.Alias;

		return alias.GetBlockElementContentTypeAlias(_shortStringHelper);
	}

	public virtual Dictionary<string, object> GetPropertyValues(GridValue.GridControl control, SyncMigrationContext context)
	{
		return new Dictionary<string, object>
		{
			{ control.Editor.Alias, control.Value ?? string.Empty }
		};
	}
}


