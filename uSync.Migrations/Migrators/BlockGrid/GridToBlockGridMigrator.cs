using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Strings;

using uSync.Migrations.Context;
using uSync.Migrations.Legacy.Grid;
using uSync.Migrations.Migrators.BlockGrid.BlockMigrators;
using uSync.Migrations.Migrators.BlockGrid.Config;
using uSync.Migrations.Migrators.BlockGrid.Content;
using uSync.Migrations.Migrators.BlockGrid.Extensions;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators.BlockGrid;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.Grid)]
[SyncMigratorVersion(7,8)]
public class GridToBlockGridMigrator : SyncPropertyMigratorBase
{
	private readonly ILegacyGridConfig _gridConfig;
	private readonly SyncBlockMigratorCollection _blockMigrators;
	private readonly ILoggerFactory _loggerFactory;
	private readonly ILogger<GridToBlockGridMigrator> _logger;

	private readonly GridConventions _conventions;

    public GridToBlockGridMigrator(
        ILegacyGridConfig gridConfig,
        SyncBlockMigratorCollection blockMigrators,
        IShortStringHelper shortStringHelper,
        ILoggerFactory loggerFactory)
    {
        _gridConfig = gridConfig;
        _blockMigrators = blockMigrators;
        _conventions = new GridConventions(shortStringHelper);
        _loggerFactory = loggerFactory;
		_logger = loggerFactory.CreateLogger<GridToBlockGridMigrator>();	
    }

    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
		=> UmbConstants.PropertyEditors.Aliases.BlockGrid;

	public override string GetDatabaseType(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
		=> nameof(ValueStorageType.Ntext);

	public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
	{
		if (dataTypeProperty.ConfigAsString == null)
			return new BlockGridConfiguration();

		var gridConfiguration = JsonConvert
			.DeserializeObject<GridConfiguration>(dataTypeProperty.ConfigAsString);

		if (gridConfiguration == null)
			return new BlockGridConfiguration();


		var legacyGridEditorsConfig = GetGridConfig(context);
		var gridToBlockContext = new GridToBlockGridConfigContext(gridConfiguration, legacyGridEditorsConfig);

		var contentBlockHelper = new GridToBlockGridConfigBlockHelper(_blockMigrators, _loggerFactory.CreateLogger<GridToBlockGridConfigBlockHelper>());
		var layoutBlockHelper = new GridToBlockGridConfigLayoutBlockHelper(_conventions, _loggerFactory.CreateLogger<GridToBlockGridConfigLayoutBlockHelper>());

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

	private ILegacyGridEditorsConfig GetGridConfig(SyncMigrationContext context)
	{
		return _gridConfig.EditorsByContext(context);
    }

    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
	{
		if (string.IsNullOrWhiteSpace(contentProperty.Value))
			return string.Empty;

		// has already been converted. 
		if (contentProperty.Value.Contains("\"Umbraco.BlockGrid\""))
		{
			_logger.LogDebug("Property [{name}] is already BlockGrid", contentProperty.EditorAlias);
			return contentProperty.Value;
		}

		var source = JsonConvert.DeserializeObject<GridValue>(contentProperty.Value);
		if (source == null)
		{
			_logger.LogDebug("Property {alias} is empty", contentProperty.EditorAlias);
			return string.Empty;
		}

		var helper = new GridToBlockContentHelper(_conventions, _blockMigrators,
			_loggerFactory.CreateLogger<GridToBlockContentHelper>());
		
		var blockValue = helper.ConvertToBlockValue(source, context);
		if (blockValue == null)
		{
			_logger.LogDebug("Converted value for {alias} is empty", contentProperty.EditorAlias);
			return string.Empty;
		}

		return JsonConvert.SerializeObject(blockValue, Formatting.Indented);
	}
}

