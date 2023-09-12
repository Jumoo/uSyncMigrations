﻿using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Umbraco.Cms.Core.Logging;
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
	private readonly IProfilingLogger _profilingLogger;
	private readonly ILogger<GridToBlockGridMigrator> _logger;
	private readonly IProfilingLogger _profilingLogger;

	private readonly GridConventions _conventions;

    public GridToBlockGridMigrator(
        ILegacyGridConfig gridConfig,
        SyncBlockMigratorCollection blockMigrators,
        IShortStringHelper shortStringHelper,
        ILoggerFactory loggerFactory,
        IProfilingLogger profilingLogger)
    {
        _gridConfig = gridConfig;
        _blockMigrators = blockMigrators;
        _conventions = new GridConventions(shortStringHelper);
        _loggerFactory = loggerFactory;
<<<<<<< HEAD
        _profilingLogger = profilingLogger;
        _logger = loggerFactory.CreateLogger<GridToBlockGridMigrator>();	
=======
        _logger = loggerFactory.CreateLogger<GridToBlockGridMigrator>();
        _profilingLogger = profilingLogger;
>>>>>>> 8b68bbf (Namespace fixes.)
    }

    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
		=> UmbConstants.PropertyEditors.Aliases.BlockGrid;

	public override string GetDatabaseType(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
		=> nameof(ValueStorageType.Ntext);

	public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
	{
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
        var layoutSettingsBlockHelper = new GridToBlockGridConfigLayoutSettingsHelper(_conventions, _loggerFactory.CreateLogger<GridToBlockGridConfigLayoutBlockHelper>());

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

<<<<<<< HEAD
		var helper = new GridToBlockContentHelper(
			_conventions, 
			_blockMigrators,
			_loggerFactory.CreateLogger<GridToBlockContentHelper>(), 
			_profilingLogger);
=======
		var helper = new GridToBlockContentHelper(_conventions, _blockMigrators,
			_loggerFactory.CreateLogger<GridToBlockContentHelper>(), _profilingLogger);
>>>>>>> 9a47735 (Namespace fixes.)
		
		var blockValue = helper.ConvertToBlockValue(source, context);
		if (blockValue == null)
		{
			_logger.LogDebug("  Converted value for {alias} is empty", contentProperty.EditorAlias);
			return string.Empty;
		}

		_logger.LogDebug("<< {method}", nameof(GetContentValue));

		return JsonConvert.SerializeObject(blockValue, Formatting.Indented);
	}
}

