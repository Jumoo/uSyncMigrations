using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

using uSync.Migrations.Core.Legacy.Grid;
using uSync.Migrations.Migrators.BlockGrid.BlockMigrators;
using uSync.Migrations.Migrators.BlockGrid.Extensions;
using uSync.Migrations.Migrators.BlockGrid.Models;
using uSync.Migrations.Migrators.BlockGrid.SettingsMigrators;

namespace uSync.Migrations.Migrators.BlockGrid.Content;

/// <summary>
///  helper class that actually gets content from the grid and puts it into the blocklist. 
/// </summary>
internal class GridToBlockContentHelper
{
    private readonly SyncBlockMigratorCollection _blockMigrators;
    private readonly GridConventions _conventions;
    private readonly ILogger<GridToBlockContentHelper> _logger;
    private readonly IProfilingLogger _profilingLogger;
    private readonly IMediaService _mediaService;

    public GridToBlockContentHelper(
        GridConventions gridConventions,
        SyncBlockMigratorCollection blockMigrators,
        ILogger<GridToBlockContentHelper> logger,
        IProfilingLogger profilingLogger,
        IMediaService mediaService)
    {
        _blockMigrators = blockMigrators;
        _conventions = gridConventions;
        _logger = logger;
        _profilingLogger = profilingLogger;
        _mediaService = mediaService;
    }

    /// <summary>
    ///  convert a grid value (so with sections, rows, areas and controls) into a block value for a grid.
    /// </summary>
    public BlockValue? ConvertToBlockValue(LegacyGridValue source, SyncMigrationContext context, string dataTypeAlias)
    {
        _logger.LogDebug(">> {method}", nameof(ConvertToBlockValue));

        // empty grid check
        if (source.Sections.Any() != true)
        {
            _logger.LogDebug("  Grid has not sections, returning null");
            return null;
        }

        var sectionContentTypeAlias = _conventions.SectionContentTypeAlias(source.Name);


        var sections = source.Sections
            .Select(x => (Grid: x.Grid.GetIntOrDefault(0), x.Rows))
            .ToArray();

        var gridColumns = sections.Sum(x => x.Grid);

        var block = new BlockValue();

        var blockLayouts = new List<BlockGridLayoutItem>();

        BlockGridLayoutItem? rootLayoutItem = null;

        if (context.ContentTypes.TryGetKeyByAlias(sectionContentTypeAlias, out var sectionKey))
        {
            var rootSection = new BlockItemData
            {
                Udi = Udi.Create(UmbConstants.UdiEntityType.Element, Guid.NewGuid()),
                ContentTypeKey = sectionKey,
                ContentTypeAlias = sectionContentTypeAlias
            };
            block.ContentData.Add(rootSection);
            rootLayoutItem = new BlockGridLayoutItem
            {
                ContentUdi = rootSection.Udi,
                ColumnSpan = gridColumns,
                RowSpan = 1
            };
        }
        var rootLayoutAreas = new List<BlockGridLayoutAreaItem>();

        foreach (var item in sections.Select((value, sectionIndex) => new { sectionIndex, value }))
        {
            var (sectionColumns, rows) = item.value;
            var sectionIsFullWidth = sectionColumns == gridColumns;

            foreach (var row in rows)
            {
                var areas = row.Areas.Select((x, i) => (x, i));

                var rowColumns = row.Areas.Sum(x => x.Grid.GetIntOrDefault(0));
                var rowIsFullWidth = sectionIsFullWidth && rowColumns == gridColumns;

                var rowLayoutAreas = new List<BlockGridLayoutAreaItem>();

                foreach (var area in row.Areas.Select((value, index) => (value, index)))
                {
                    var areaIsFullWidth = rowIsFullWidth && area.value.Grid.GetIntOrDefault(0) == gridColumns;

                    // get the content
                    var content = GetGridAreaBlockContent(area.value, context).ToList();
                    if (!content.Any()) continue;

                    var settings = GetSettingsBlockItemDataFromArea(area.value, context, dataTypeAlias);

                    // get the layouts 
                    var layouts = GetGridAreaBlockLayouts(area.value, content).ToList();
                    if (!layouts.Any()) continue;

                    if (settings is not null)
                    {
                        var areaSettingsContentItem = new BlockItemData()
                        {
                            Udi = Udi.Create(UmbConstants.UdiEntityType.Element, Guid.NewGuid()),
                            ContentTypeKey = context.ContentTypes.GetKeyByAlias(_conventions.AreaSettingsElementTypeAlias)
                        };

                        content.Add(areaSettingsContentItem);

                        var areaSettingsItem = new BlockGridLayoutItem()
                        {
                            ContentUdi = areaSettingsContentItem.Udi,
                            ColumnSpan = gridColumns,
                            RowSpan = 1,
                            SettingsUdi = settings.Udi
                        };

                        layouts.Insert(0, areaSettingsItem);

                        block.SettingsData.Add(settings);
                    }

                    if (areaIsFullWidth)
                    {
                        blockLayouts.AddRange(layouts);
                    }
                    else
                    {
                        var areaItem = new BlockGridLayoutAreaItem
                        {
                            Key = _conventions.LayoutAreaAlias(row.Name!, _conventions.AreaAlias(area.index)).ToGuid(),
                            Items = layouts.ToArray()
                        };

                        rowLayoutAreas.Add(areaItem);
                    }

                    // add the content to the block. 
                    content.ForEach(x =>
                    {
                        block.ContentData.Add(x);
                    });
                }

                // row 
                if (!rowLayoutAreas.Any()) continue;

                var rowContentAndSettings = GetGridRowBlockContentAndSettings(row, context, dataTypeAlias);

                block.ContentData.Add(rowContentAndSettings.Content);

                if (rowContentAndSettings.Settings is not null)
                {
                    block.SettingsData.Add(rowContentAndSettings.Settings);
                }
                blockLayouts.Add(GetGridRowBlockLayout(rowContentAndSettings, rowLayoutAreas, rowColumns));
            }

            // section 
            if (!sectionIsFullWidth)
            {
                var areaItem = new BlockGridLayoutAreaItem
                {
                    Key = _conventions.GridAreaConfigAlias(_conventions.AreaAlias(item.sectionIndex)).ToGuid(),
                    Items = blockLayouts.ToArray()
                };
                rootLayoutAreas.Add(areaItem);
                blockLayouts.Clear();
            }
        }
        if (rootLayoutItem != null && rootLayoutAreas.Count > 1)
        {
            rootLayoutItem.Areas = rootLayoutAreas.ToArray();
            blockLayouts.Add(rootLayoutItem);
        }

        // end - process layouts into block format. 
        block.Layout = new Dictionary<string, JToken>
        {
            { UmbConstants.PropertyEditors.Aliases.BlockGrid, JToken.FromObject(blockLayouts) }
        };

        _logger.LogDebug(">> {method}", nameof(ConvertToBlockValue));

        return block;
    }

    private IEnumerable<BlockItemData> GetGridAreaBlockContent(GridValue.GridArea area, SyncMigrationContext context)
    {
        foreach (var control in area.Controls)
        {
            var content = GetBlockItemDataFromGridControl(control, context);
            if (content == null) continue;

            yield return content;
        }
    }

    private BlockItemData? GetSettingsBlockItemDataFromArea(GridValue.GridArea area, SyncMigrationContext context, string dataTypeAlias)
    {
        if (dataTypeAlias.IsNullOrWhiteSpace())
        {
            return null;
        }

        if ((area.Config is null || area.Config.Count() == 0) &&
            (area.Styles is null || area.Styles.Count() == 0))
        {
            // avoid adding a settings container if there aren't any settings
            return null;
        }

        var settingsValues = new Dictionary<string, object?>();

        var areaLayoutSettingsContentTypeAlias = _conventions.LayoutAreaSettingsContentTypeAlias(dataTypeAlias);
        var areaSettingsContentTypeKey = context.GetContentTypeKeyOrDefault(areaLayoutSettingsContentTypeAlias, areaLayoutSettingsContentTypeAlias.ToGuid());
        var areaSettingsContentType = context.ContentTypes.GetNewContentTypes().FirstOrDefault(t => t.Alias == areaLayoutSettingsContentTypeAlias);

        if (area.Config is not null)
        {
            foreach (JProperty config in area.Config)
            {
                AddPropertyValue(context, areaSettingsContentType, settingsValues, config);
            }
        }

        if (area.Styles is not null)
        {
            foreach (JProperty style in area.Styles)
            {
                AddPropertyValue(context, areaSettingsContentType, settingsValues, style);
            }
        }

        return new BlockItemData
        {
            Udi = Udi.Create(UmbConstants.UdiEntityType.Element, Guid.NewGuid()),
            ContentTypeKey = areaSettingsContentTypeKey,
            ContentTypeAlias = areaLayoutSettingsContentTypeAlias,
            RawPropertyValues = settingsValues
        };
    }

    private IEnumerable<BlockGridLayoutItem> GetGridAreaBlockLayouts(GridValue.GridArea area, IEnumerable<BlockItemData> content)
    {
        foreach (var item in content)
        {
            var layout = new BlockGridLayoutItem
            {
                ContentUdi = item.Udi,
                SettingsUdi = null,
                ColumnSpan = area.Grid.GetIntOrDefault(0),
                RowSpan = 1
            };

            yield return layout;
        }
    }

    private BlockContentPair GetGridRowBlockContentAndSettings(LegacyGridValue.LegacyGridRow row, SyncMigrationContext context, string dataTypeAlias)
    {
        var rowLayoutContentTypeAlias = _conventions.LayoutContentTypeAlias(row.Name);
        var rowContentTypeKey = context.GetContentTypeKeyOrDefault(rowLayoutContentTypeAlias, rowLayoutContentTypeAlias.ToGuid());

        var contentData = new BlockItemData
        {
            Udi = Udi.Create(UmbConstants.UdiEntityType.Element, row.Id),
            ContentTypeKey = rowContentTypeKey,
            ContentTypeAlias = rowLayoutContentTypeAlias
        };

        var settingsData = GetSettingsBlockItemDataFromRow(row, context, dataTypeAlias, contentData.Udi);

        return new BlockContentPair(content: contentData, settings: settingsData);
    }
    private BlockItemData? GetSettingsBlockItemDataFromRow(LegacyGridValue.LegacyGridRow row, SyncMigrationContext context, string dataTypeAlias, Udi contentUdi)
    {
        if (dataTypeAlias.IsNullOrWhiteSpace())
        {
            return null;
        }

        var settingsValues = new Dictionary<string, object?>();

        var rowLayoutSettingsContentTypeAlias = _conventions.LayoutSettingsContentTypeAlias(dataTypeAlias);
        var rowSettingsContentTypeKey = context.GetContentTypeKeyOrDefault(rowLayoutSettingsContentTypeAlias, rowLayoutSettingsContentTypeAlias.ToGuid());
        var rowSettingsContentType = context.ContentTypes.GetNewContentTypes().FirstOrDefault(t => t.Alias == rowLayoutSettingsContentTypeAlias);

        if (row.Config is not null)
        {
            foreach (JProperty config in row.Config)
            {
                AddPropertyValue(context, rowSettingsContentType, settingsValues, config);
            }
        }

        if (row.Styles is not null)
        {
            foreach (JProperty style in row.Styles)
            {
                AddPropertyValue(context, rowSettingsContentType, settingsValues, style);
            }
        }

        return new BlockItemData
        {
            Udi = contentUdi,
            ContentTypeKey = rowSettingsContentTypeKey,
            ContentTypeAlias = rowLayoutSettingsContentTypeAlias,
            RawPropertyValues = settingsValues
        };
    }

    private void AddPropertyValue(SyncMigrationContext context, NewContentTypeInfo? rowSettingsContentType, Dictionary<string, object?> settingsValues, JProperty config)
    {
        var formattedKey = _conventions.FormatGridSettingKey(config.Name);

        if (settingsValues.ContainsKey(formattedKey))
        {
            // Dont overwrite values. If styles have same settings keys as config, what should happen?
            // TODO: Figure out what to do here / what gets priority?##
            return;
        }

        var configValue = config.Value;

        // For radio button list data types, replace the value with the label, since Umbraco radio button lists don't have separate labels and values.
        // The new value (the old label) will need to be translated to the old value in the presentation code.
        if (rowSettingsContentType != null)
        {
            var property = rowSettingsContentType.Properties.FirstOrDefault(p => p.Alias == config.Name);

            if (property != null)
            {
                var dataType = context.DataTypes.GetNewDataType(property.DataTypeAlias);

                if (dataType != null &&
                    dataType.Config is RadioButtonListConfig)
                {
                    var radioButtonListConfig = (dataType.Config as RadioButtonListConfig)!;
                    var configItem = radioButtonListConfig.Items.FirstOrDefault(i => i.OldValue == configValue.Value<string>());
                    if (configItem != null)
                    {
                        configValue = JToken.Parse(string.Format("'{0}'", configItem.Value));
                    }
                }
                else if (property.DataTypeAlias == "Image Media Picker")
                {
                    Match match = (new Regex(@"^url\((.*)\)$")).Match(config.Value.ToString());
                    
                    if (match.Success)
                    {
                        string path = match.Groups[1].Value;
                        IMedia? media = _mediaService.GetMediaByPath(config.Value.ToString().Replace("url(", "").Replace(")", ""));

                        if (media is not null)
                        {
                            configValue = JToken.Parse(string.Format("[{0}]", JsonConvert.SerializeObject(new { key = Guid.NewGuid(), mediaKey = media.Key })));
                        }
                    }
                }
            }
        }

        settingsValues.Add(formattedKey, configValue);
    }


    private BlockGridLayoutItem GetGridRowBlockLayout(BlockContentPair rowContentAndSettings, List<BlockGridLayoutAreaItem> rowLayoutAreas, int? rowColumns)
    {
        return new BlockGridLayoutItem
        {
            ContentUdi = rowContentAndSettings.Content.Udi,
            SettingsUdi = rowContentAndSettings.Settings?.Udi,
            Areas = rowLayoutAreas.ToArray(),
            ColumnSpan = rowColumns,
            RowSpan = 1,
        };
    }

    private BlockItemData? GetBlockItemDataFromGridControl(LegacyGridValue.LegacyGridControl control, SyncMigrationContext context)
    {
        if (control.Value == null) return null;

        var blockMigrator = _blockMigrators.GetMigrator(control.Editor.Alias);
        if (blockMigrator == null)
        {
            _logger.LogWarning("No Block Migrator for [{editor}/{view}]", control.Editor.Alias, control.Editor.View);
            return null;
        }

        var contentTypeAlias = blockMigrator.GetContentTypeAlias(control);
        if (contentTypeAlias == null)
        {
            _logger.LogWarning("No contentTypeAlias from migrator {migrator}", blockMigrator.GetType().Name);
            return null;
        }

        if (control.Editor.Alias.InvariantEquals("macro"))
        {
            var macroObject = JsonConvert.DeserializeObject<MacroObject>(control.Value.ToString());

            if (macroObject != null)
            {
                contentTypeAlias = macroObject.MacroEditorAlias;
            }
        }

        if (context.ContentTypes.TryGetKeyByAlias(contentTypeAlias, out var contentTypeKey) is false)
        {
            _logger.LogWarning("Cannot find content type key from alias {alias}", contentTypeAlias);
            return null;
        }

        var data = new BlockItemData
        {
            Udi = Udi.Create(UmbConstants.UdiEntityType.Element, Guid.NewGuid()),
            ContentTypeAlias = contentTypeAlias,
            ContentTypeKey = contentTypeKey
        };


        foreach (var (propertyAlias, value) in blockMigrator.GetPropertyValues(control, context))
        {
            if (context.ContentTypes.TryGetEditorAliasByTypeAndProperty(contentTypeAlias, propertyAlias, out var editorAlias) is false) { continue; }

            var propertyValue = value;

            if (context.Migrators.TryGetMigrator(editorAlias.OriginalEditorAlias, out var migrator) is true)
            {
                var property = new SyncMigrationContentProperty(
                    contentTypeAlias, propertyAlias,
                    editorAlias.OriginalEditorAlias, value?.ToString() ?? string.Empty);
                propertyValue = migrator.GetContentValue(property, context);
                _logger.LogDebug("Migrator: {migrator} returned {value}", migrator.GetType().Name, propertyValue);
            }
            else
            {
                _logger.LogDebug("No Block Migrator found for [{alias}] (value will be passed through)", editorAlias.OriginalEditorAlias);
            }

            data.RawPropertyValues[propertyAlias] = propertyValue;
        }

        return data;
    }
}

internal static class GridToBlockContentExtensions
{
    public static int GetIntOrDefault(this string? value, int defaultValue)
        => int.TryParse(value, out var intValue) ? intValue : defaultValue;
}
