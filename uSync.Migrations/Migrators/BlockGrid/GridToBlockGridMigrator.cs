using Newtonsoft.Json;

using Umbraco.Cms.Core.Configuration.Grid;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Strings;

using uSync.Migrations.Migrators.BlockGrid.BlockMigrators;
using uSync.Migrations.Migrators.BlockGrid.Content;
using uSync.Migrations.Migrators.BlockGrid.Config;
using uSync.Migrations.Migrators.BlockGrid.Extensions;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Context;

namespace uSync.Migrations.Migrators.BlockGrid;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.Grid)]
[SyncMigratorVersion(8)]
public class GridToBlockGridMigrator : SyncPropertyMigratorBase
{
	private readonly IGridConfig _gridConfig;
	private readonly SyncBlockMigratorCollection _blockMigrators;

	private readonly GridConventions _conventions;

	public GridToBlockGridMigrator(
		IGridConfig gridConfig,
		SyncBlockMigratorCollection blockMigrators,
		IShortStringHelper shortStringHelper)
	{
		_gridConfig = gridConfig;
		_blockMigrators = blockMigrators;
		_conventions = new GridConventions(shortStringHelper);
	}

	public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
		=> UmbConstants.PropertyEditors.Aliases.BlockGrid;

	public override string GetDatabaseType(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
		=> nameof(ValueStorageType.Ntext);

	public override object GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
	{
		if (dataTypeProperty.ConfigAsString == null)
			return new BlockGridConfiguration();

		var gridConfiguration = JsonConvert
			.DeserializeObject<GridConfiguration>(dataTypeProperty.ConfigAsString);

		if (gridConfiguration == null)
			return new BlockGridConfiguration();

		var gridToBlockContext = new GridToBlockGridConfigContext(gridConfiguration, _gridConfig);

		var contentBlockHelper = new GridToBlockGridConfigBlockHelper(_blockMigrators);
		var layoutBlockHelper = new GridToBlockGridConfigLayoutBlockHelper(_conventions);

		// adds content types for the core elements (headline, rte, etc)
		contentBlockHelper.AddConfigDataTypes(gridToBlockContext, context);

		// prep the layouts 
		layoutBlockHelper.AddLayoutBlocks(gridToBlockContext, context);

		// Add the content blocks
		contentBlockHelper.AddContentBlocks(gridToBlockContext, context);

		// Get resultant configuration
		var result = gridToBlockContext.ConvertToBlockGridConfiguration();

		// Make sure all the block elements have been added to the migration context.
		context.ContentTypes.AddElementTypes(result.Blocks.Select(x => x.ContentElementTypeKey), true);

		return result;
	}

	public override string GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
	{
		if (string.IsNullOrWhiteSpace(contentProperty.Value))
			return string.Empty;

		// has already been converted. 
		if(contentProperty.Value.Contains("\"Umbraco.BlockGrid\"")) 
			return contentProperty.Value;

		var source = JsonConvert.DeserializeObject<GridValue>(contentProperty.Value);
		if (source == null) return string.Empty;

		var helper = new GridToBlockContentHelper(_conventions, _blockMigrators);
		var blockValue = helper.ConvertToBlockValue(source, context);

		if (blockValue == null) return string.Empty;

		return JsonConvert.SerializeObject(blockValue, Formatting.Indented);
	}
}

