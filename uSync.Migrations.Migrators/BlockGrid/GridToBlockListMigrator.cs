using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUglify.Helpers;
using System.Reflection.Metadata.Ecma335;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

using uSync.Migrations.Core.Legacy.Grid;

using uSync.Migrations.Migrators.BlockGrid.BlockMigrators;
using uSync.Migrations.Migrators.BlockGrid.Config;
using uSync.Migrations.Migrators.BlockGrid.Content;
using uSync.Migrations.Migrators.BlockGrid.Extensions;
using uSync.Migrations.Migrators.BlockGrid.SettingsMigrators;
using static uSync.Migrations.Migrators.Core.BlockListMigrator;
using GridConfiguration = uSync.Migrations.Migrators.BlockGrid.Models.GridConfiguration;

namespace uSync.Migrations.Migrators.BlockGrid;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.Grid)]
[SyncMigratorVersion(7, 8)]
public class GridToBlockListMigrator : GridToBlockGridMigrator
{
	public GridToBlockListMigrator(
		ILegacyGridConfig gridConfig,
		SyncBlockMigratorCollection blockMigrators,
		GridSettingsViewMigratorCollection gridSettingsMigrators,
		IShortStringHelper shortStringHelper,
		ILoggerFactory loggerFactory,
		IProfilingLogger profilingLogger,
		IMediaService mediaService) : base(gridConfig, blockMigrators, gridSettingsMigrators, shortStringHelper, loggerFactory, profilingLogger, mediaService)
	{
	}

	public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
		=> UmbConstants.PropertyEditors.Aliases.BlockList;

	public override string GetDatabaseType(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
		=> nameof(ValueStorageType.Ntext);

	public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
	{
		var blockGridConfig = base.GetConfigValues(dataTypeProperty, context) as BlockGridConfiguration;

		if (blockGridConfig == null)
		{
			return new BlockListConfiguration();
		}

		var config = new BlockListConfiguration()
		{
			Blocks = blockGridConfig.Blocks
			// Don't include layout or grid settings blocks (these will have no "BlockPreviewer" view assigned by default)
			.Where(x => x.Areas.Any() == false && x.View != null)
			.Select(x => new BlockListConfiguration.BlockConfiguration()
			{
				ContentElementTypeKey = x.ContentElementTypeKey,
				SettingsElementTypeKey = x.SettingsElementTypeKey,
				EditorSize = x.EditorSize,
				ForceHideContentEditorInOverlay = x.ForceHideContentEditorInOverlay,
				Label = x.Label,
				Stylesheet = x.Stylesheet,
				Thumbnail = x.Thumbnail,
				View = x.View,

				// No need to set the colors when we're not importing layout blocks
				BackgroundColor = null, //x.BackgroundColor
				IconColor = null, //x.IconColor
			}).ToArray(),
			MaxPropertyWidth = blockGridConfig.MaxPropertyWidth,
			UseLiveEditing = blockGridConfig.UseLiveEditing,
			UseInlineEditingAsDefault = false,
			UseSingleBlockMode = false,
			ValidationLimit = new BlockListConfiguration.NumberRange() { Min = blockGridConfig.ValidationLimit.Min, Max = blockGridConfig.ValidationLimit.Max }
		};

		return config;
	}

	public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
	{
		var gridStrValue = base.GetContentValue(contentProperty, context);
		if (string.IsNullOrWhiteSpace(gridStrValue))
		{
			return null;
		}

		var gridValue = JsonConvert.DeserializeObject<BlockValue>(gridStrValue);
		if (gridValue == null)
		{
			return null;
		}

		var blockList = new BlockListValue()
		{
			ContentData = gridValue.ContentData?.Select(x => new BlockListRowValue()
			{
				ContentTypeKey = x.ContentTypeKey,
				RawPropertyValues = x.RawPropertyValues,
				Udi = x.Udi?.ToString(),
			}).ToArray(),
			Layout = new BlockListLayoutValue()
			{
				BlockOrder = gridValue.Layout["Umbraco.BlockGrid"].Select(x => new BlockUdiValue()
				{
					ContentUdi = x["contentUdi"]?.Value<string>(),
					SettingsUdi = x["settingsUdi"]?.Value<string>()
				}).ToArray()
			},
			SettingsData = gridValue.SettingsData?.Select(x => new BlockListRowValue()
			{
				ContentTypeKey = x.ContentTypeKey,
				RawPropertyValues = x.RawPropertyValues,
				Udi = x.Udi?.ToString(),
			}).ToArray()
		};

		return JsonConvert.SerializeObject(blockList, Formatting.Indented);
	}

}

