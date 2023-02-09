using Umbraco.Cms.Core.Configuration.Grid;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;
using uSync.Migrations.Context;
using uSync.Migrations.Migrators.BlockGrid.BlockMigrators;
using uSync.Migrations.Migrators.BlockGrid.Extensions;

namespace uSync.Migrations.Migrators.BlockGrid.Config;
internal class GridToBlockGridConfigBlockHelper
{
    private readonly SyncBlockMigratorCollection _syncBlockMigrators;

	public GridToBlockGridConfigBlockHelper(
		SyncBlockMigratorCollection syncBlockMigrators)
	{
		_syncBlockMigrators = syncBlockMigrators;
	}

	/// <summary>
	///  adds any datatypes we need to make the grid. 
	/// </summary>
	/// <param name="context"></param>
	public void AddConfigDataTypes(GridToBlockGridConfigContext gridToBlockContext, SyncMigrationContext context)
    {
        // add each thing in the config, as a 'new' doctype
        // the content handler will then either create or ignore these if they are already there. 
        foreach (var editor in gridToBlockContext.GridConfig.EditorsConfig.Editors)
        {
            if (editor.View == null) continue;

            var blockMigrator = _syncBlockMigrators.GetMigrator(editor);
            if (blockMigrator == null) continue;

            var additionalContentTypes = blockMigrator.AdditionalContentTypes(editor);
            if (additionalContentTypes == null) continue;

            foreach(var newContentType in additionalContentTypes)
            {
                context.ContentTypes.AddNewContentType(newContentType);
                context.ContentTypes.AddAliasAndKey(newContentType.Alias, newContentType.Key);
            }
        }
    }

    public void AddContentBlocks(GridToBlockGridConfigContext gridBlockContext, SyncMigrationContext context)
    {
        // add the grid elements to the block config 
        var gridContentTypeKeys = AddGridContentBlocksToConfig(gridBlockContext.GridConfig, gridBlockContext, context);

        // now add the editor to the right bit of the blocks.
        AddEditorsToAllowedAreasInBlocks(gridContentTypeKeys, gridBlockContext, context);

    }


    private void AddEditorsToAllowedAreasInBlocks(Dictionary<string, Guid[]> gridContentTypeKeys, GridToBlockGridConfigContext gridBlockContext, SyncMigrationContext migrationContext)
    {
        foreach (var (area, gridEditorAliases) in gridBlockContext.AllowedEditors)
        {
            var allowedElementTypes = new List<Guid>();
            foreach (var gridEditorAlias in gridEditorAliases)
            {
                if (gridContentTypeKeys.ContainsKey(gridEditorAlias))
                {
                    allowedElementTypes.AddRange(gridContentTypeKeys[gridEditorAlias]);
                }
            }

            var allowedBlocks = gridBlockContext.ContentBlocks
                .Where(x => allowedElementTypes.Contains(x.ContentElementTypeKey))
                .ToArray();

            if (!allowedBlocks.Any()) continue;

            if (area == gridBlockContext.RootArea)
            {
                foreach (var block in allowedBlocks)
                {
                    block.AllowAtRoot = true;
                }
                continue;
            }

            area.SpecifiedAllowance = allowedBlocks
                .Select(x => new BlockGridConfiguration.BlockGridAreaConfigurationSpecifiedAllowance
                {
                    ElementTypeKey = x.ContentElementTypeKey,
                }).ToArray();
        }
    }

    /// <summary>
    ///  returns all the allowed/known content types for a section of the grid.
    /// </summary>
    private Dictionary<string, Guid[]> AddGridContentBlocksToConfig(IGridConfig gridConfig, GridToBlockGridConfigContext gridBlockContext, SyncMigrationContext context)
    {
        var referencedEditors = gridBlockContext.AllEditors().ToList();

        var allowedContentTypes = new Dictionary<string, Guid[]>();

        foreach (var editor in gridConfig.EditorsConfig.Editors
            .Where(x => referencedEditors.Contains("*") ||
            referencedEditors.InvariantContains(x.Alias)))
        {
            var blocks = editor.ConvertToBlockGridBlocks(context, 
                _syncBlockMigrators,
                gridBlockContext.GridBlocksGroup.Key);

            gridBlockContext.ContentBlocks.AddRange(blocks);

            allowedContentTypes[editor.Alias] = blocks.Select(x => x.ContentElementTypeKey).ToArray();
        }

        return allowedContentTypes;
    }


}
