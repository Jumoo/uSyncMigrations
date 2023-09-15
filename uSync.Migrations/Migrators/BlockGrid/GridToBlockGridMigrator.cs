using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using uSync.Migrations.Configuration;
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
	private readonly GridSettingsViewMigratorCollection _gridSettingsMigrators;
	private readonly ILoggerFactory _loggerFactory;
	private readonly ILogger<GridToBlockGridMigrator> _logger;
	private readonly IProfilingLogger _profilingLogger;
	private readonly GridConventions _conventions;
	private readonly IContentTypeService _contentTypeService;
	private readonly IDataTypeService _dataTypeService;

    public GridToBlockGridMigrator(
        ILegacyGridConfig gridConfig,
        SyncBlockMigratorCollection blockMigrators,
		GridSettingsViewMigratorCollection gridSettingsMigrators,
        IShortStringHelper shortStringHelper,
        ILoggerFactory loggerFactory,
        IProfilingLogger profilingLogger,
		IContentTypeService contentTypeService,
		IDataTypeService dataTypeService
		)
    {
        _gridConfig = gridConfig;
        _blockMigrators = blockMigrators;
		_gridSettingsMigrators = gridSettingsMigrators;
        _conventions = new GridConventions(shortStringHelper);
        _loggerFactory = loggerFactory;
        _profilingLogger = profilingLogger;
		_logger = loggerFactory.CreateLogger<GridToBlockGridMigrator>();
		_contentTypeService = contentTypeService;
		_dataTypeService = dataTypeService;
    }

    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
		=> UmbConstants.PropertyEditors.Aliases.BlockGrid;

	public override string GetDatabaseType(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
		=> nameof(ValueStorageType.Ntext);

	public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
	{
		//migrationDataTypeProperty = dataTypeProperty;	
        _logger.LogDebug(">> {method}", nameof(GetConfigValues));

        if (dataTypeProperty.ConfigAsString == null)
		{
			_logger.LogDebug("   Config is null, returning empty block grid config");
			return new BlockGridConfiguration();
		}

		var gridConfiguration = JsonConvert
			.DeserializeObject<GridConfiguration>(dataTypeProperty.ConfigAsString);

		if (gridConfiguration == null)
		{
			_logger.LogDebug("   Grid Config is null, returning empty block grid config");
			return new BlockGridConfiguration();
		}


		var legacyGridEditorsConfig = GetGridConfig(context);
		var gridToBlockContext = new GridToBlockGridConfigContext(gridConfiguration, legacyGridEditorsConfig);

		var contentBlockHelper = new GridToBlockGridConfigBlockHelper(_blockMigrators, _loggerFactory.CreateLogger<GridToBlockGridConfigBlockHelper>());
		var layoutBlockHelper = new GridToBlockGridConfigLayoutBlockHelper(_conventions, _loggerFactory.CreateLogger<GridToBlockGridConfigLayoutBlockHelper>());
        var layoutSettingsBlockHelper = new GridToBlockGridConfigLayoutSettingsHelper(_conventions, _gridSettingsMigrators, _loggerFactory.CreateLogger<GridToBlockGridConfigLayoutSettingsHelper>());

		// adds content types for the core elements (headline, rte, etc)
		contentBlockHelper.AddConfigDataTypes(gridToBlockContext, context);
		
		// Add settings
		layoutSettingsBlockHelper.AddGridSettings(gridToBlockContext, context, dataTypeProperty.DataTypeAlias);

		// prep the layouts 
		layoutBlockHelper.AddLayoutBlocks(gridToBlockContext, context, dataTypeProperty.DataTypeAlias);

		// Add the content blocks
		contentBlockHelper.AddContentBlocks(gridToBlockContext, context);

		// Get resultant configuration
		var result = gridToBlockContext.ConvertToBlockGridConfiguration();

		// Make sure all the block elements have been added to the migration context.
		context.ContentTypes.AddElementTypes(result.Blocks.Select(x => x.ContentElementTypeKey), true);

        _logger.LogDebug("<< {method}", nameof(GetConfigValues));

        return result;
	}

	private ILegacyGridEditorsConfig GetGridConfig(SyncMigrationContext context)
	{
		return _gridConfig.EditorsByContext(context);
    }

    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
	{
		_logger.LogDebug(">> {method}", nameof(GetContentValue));

		var gridDataTypeId = _contentTypeService.Get(contentProperty.ContentTypeAlias)?.PropertyTypes
			.Where(propertyType => propertyType.PropertyEditorAlias == contentProperty.EditorAlias && propertyType.Alias == contentProperty.PropertyAlias)
			.Select(propertyType => propertyType.DataTypeId)
			.FirstOrDefault();

		var dataTypeAlias = "";
		if (gridDataTypeId is not null)
		{
			dataTypeAlias = _dataTypeService.GetDataType((int)gridDataTypeId)?.Name;
        }
		else
		{
			_logger.LogWarning("  Data type for grid could not be found when converting {alias}. Migration will run, but layout setting will not be migrated.", contentProperty.EditorAlias);
		}

        if (string.IsNullOrWhiteSpace(contentProperty.Value))
		{
			_logger.LogDebug("  Content property is blank, nothing to migrate");
			return string.Empty;
		}

		// has already been converted. 
		if (contentProperty.Value.Contains("\"Umbraco.BlockGrid\""))
		{
			_logger.LogDebug("  Property [{name}] is already BlockGrid", contentProperty.EditorAlias);
			return contentProperty.Value;
		}

		var source = JsonConvert.DeserializeObject<GridValue>(contentProperty.Value);
		if (source == null)
		{
			_logger.LogDebug("  Property {alias} is empty", contentProperty.EditorAlias);
			return string.Empty;
		}

		// For some reason, DTGEs can sometimes end up without a view specified. This should fix it.
		foreach (var section in source.Sections)
		{
			foreach (var row in section.Rows)
			{
				foreach (var area in row.Areas)
				{
					foreach (var control in area.Controls)
					{
						if (control.Editor.View == null && control.Value is JObject value && value["dtgeContentTypeAlias"] != null)
						{
							control.Editor.View = "/App_Plugins/DocTypeGridEditor/Views/doctypegrideditor.html";
							_logger.LogDebug("Control {alias} looks like a DTGE, but has no view, {view} has been added as view", control.Editor.Alias, control.Editor.View);
						}
					}
				}
			}
		}

		var helper = new GridToBlockContentHelper(
			_conventions, 
			_blockMigrators,
			_loggerFactory.CreateLogger<GridToBlockContentHelper>(), 
			_profilingLogger);



		var blockValue = helper.ConvertToBlockValue(source, context, dataTypeAlias ?? "");
		if (blockValue == null)
		{
			_logger.LogDebug("  Converted value for {alias} is empty", contentProperty.EditorAlias);
			return string.Empty;
		}

		_logger.LogDebug("<< {method}", nameof(GetContentValue));

		return JsonConvert.SerializeObject(blockValue, Formatting.Indented);
	}
}

