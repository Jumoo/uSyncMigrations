using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Lucene.Net.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Grid;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Extensions;
using uSync.Migrations.Composing;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Models;
using Attribute = Lucene.Net.Util.Attribute;

namespace uSync.Migrations.Migrators.Optional;

[SyncDefaultMigrator]
[SyncMigrator(UmbConstants.PropertyEditors.Aliases.Grid)]
[SyncMigratorVersion(7,8)]
public class GridToBlockGridMigrator : SyncPropertyMigratorBase
{
    public static string GetBlockSettingsContentTypeAlias(IShortStringHelper shortStringHelper, string name) =>
        GetContentTypeAlias(shortStringHelper, "BlockSettings_", name);

    public static string GetBlockGridLayoutContentTypeAlias(IShortStringHelper shortStringHelper, string name) =>
        GetContentTypeAlias(shortStringHelper, "BlockGridLayout_", name);

    public static string GetBlockGridAreaConfigurationAlias(IShortStringHelper shortStringHelper, string name) =>
        GetContentTypeAlias(shortStringHelper, "BlockGridArea_", name);
    
    
    private static string GetContentTypeAlias(IShortStringHelper shortStringHelper, string prefix, string name)
    {
        return $"{prefix}{name}".ToSafeAlias(shortStringHelper);
    }

    private static Guid GetGuidFromAlias(string alias)
    {
        using var algorithm = HashAlgorithm.Create(nameof(MD5));
        var buffer = Encoding.UTF8.GetBytes(alias);
        var hash = algorithm!.ComputeHash(buffer);
        var hex = Convert.ToHexString(hash);
        var result = new Guid(hex);
        return result;
    }
    
    private readonly IGridConfig _gridConfig;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly SyncMigrationHandlerCollection _migrationHandlers;

    public GridToBlockGridMigrator(IGridConfig gridConfig, SyncMigrationHandlerCollection migrationHandlers, IShortStringHelper shortStringHelper)
    {
        _gridConfig = gridConfig;
        _migrationHandlers = migrationHandlers;
        _shortStringHelper = shortStringHelper;
    }
    
    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.BlockGrid;

    public override string GetDatabaseType(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => nameof(ValueStorageType.Ntext);

    public override object GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        GridConfiguration? gridConfiguration = null!;

        switch (context.SourceVersion)
        {
            case 7:
            {
                if (dataTypeProperty.PreValues != null)
                {
                    gridConfiguration = new GridConfiguration()
                        .MapPreValues(dataTypeProperty.PreValues);

                }
                break;
            }
            case 8:
            {
                if (!string.IsNullOrWhiteSpace(dataTypeProperty.ConfigAsString))
                {
                    gridConfiguration = JsonConvert
                        .DeserializeObject<GridConfiguration>(dataTypeProperty.ConfigAsString);
                }

                break;
            }
        }

        if (gridConfiguration == null)
        {
            return null!;
        }

        var blockGridConfiguration = ConvertToBlockGridConfiguration(gridConfiguration, context);

        return blockGridConfiguration;
    }

    private object ConvertToBlockGridConfiguration(GridConfiguration? gridConfiguration, SyncMigrationContext context)
    {
        var result = new BlockGridConfiguration();

        if (gridConfiguration.Items?.TryGetValue("styles", out var styles) ==true )
        {
            /*
            [
            {
                "label": "Background color",
                "description": "Choose background color",
                "key": "background-color",
                "view": "colorpicker",
                "modifier": "#{0}",
                "prevalues": [
                {
                    "label": "City Blue",
                    "value": "#14143C"
                },
                {
                    "label": "Summer Red",
                    "value": "#EB3755"
                },
            */
        }

        if (gridConfiguration.Items?.TryGetValue("config", out var config) ==true )
        {
            // applyTo
            // view: map to a known (new?) datatype
            
            /*
"config": [
    {
      "label": "Top spacing",
      "description": "Adds spacing to the top of the row",
      "key": "top-spacing",
      "view": "boolean",
      "applyTo": "row"
    },
    {
      "label": "Bottom spacing",
      "description": "Adds spacing to the bottom of the row",
      "key": "bottom-spacing",
      "view": "boolean",
      "applyTo": "row"
    },
            */
        }

        if (gridConfiguration.Items?.TryGetValue("columns", out var columns) ==true )
        {
            // columns = the number of columns...
            result.GridColumns = columns.Value<int>();
        }

        var blockGroups = new List<BlockGridConfiguration.BlockGridGroupConfiguration>();

        var layoutBlocks = new Dictionary<string, BlockGridConfiguration.BlockGridBlockConfiguration>();

        var layoutsGroup = new BlockGridConfiguration.BlockGridGroupConfiguration
        {
            Key = GetGuidFromAlias("group_" + nameof(layoutBlocks)),
            Name = "Layouts",
        };
        
        blockGroups.Add(layoutsGroup);

        var rootArea = new BlockGridConfiguration.BlockGridAreaConfiguration();
        var allowedEditors = new Dictionary<BlockGridConfiguration.BlockGridAreaConfiguration, IEnumerable<string>>();
        var allowedLayouts = new Dictionary<BlockGridConfiguration.BlockGridAreaConfiguration, IEnumerable<string>>();

        // templates are called layouts in UI
        // Layouts are the overall work area for the grid editor, usually you only need one or two different layouts
        if (gridConfiguration.Items.TryGetValue("templates", out var templates))
        {
            /*
"templates": [
    {
      "name": "1 column layout",
      "sections": [
        {
          "grid": 12,
          "allowAll": false,
          "allowed": [
            "Full width",
            "2 Columns",
            "2 Columns 4-8",
            "2 Columns 8-4",
            "2 Columns 7-5",
            "3 Columns",
            "2 Columns 5-7",
            "4 Columns"
          ]
        }
      ]
    },
    {
      "name": "2 column layout",
      "sections": [
        {
          "grid": 6,
          "allowAll": false,
          "allowed": [
            "Full width"
          ]
        },
        {
          "grid": 6,
          "allowAll": false,
          "allowed": [
            "Full width"
          ]
        }
      ]
    }
  ],
             */

            var gridTemplateConfigurations = templates
                .ToObject<IEnumerable<GridTemplateConfiguration>>() ?? Enumerable.Empty<GridTemplateConfiguration>();

            foreach (var template in gridTemplateConfigurations)
            {
                if (template.Sections == null)
                {
                    continue;
                }
                
                // create new layout
                var areas = new List<BlockGridConfiguration.BlockGridAreaConfiguration>();

                foreach (var (section, index) in template.Sections.Select((x, i) => (x, i)))
                {
                    var allowed = new List<string>();
                    
                    if (section.Allowed?.Any() == true)
                    {
                        allowed.AddRange(section.Allowed);
                    }
                    else
                    {
                        allowed.Add("*");
                    }

                    if (section.Grid == result.GridColumns)
                    {
                        if (allowedLayouts.TryGetValue(rootArea, out var rootAllowed))
                        {
                            allowed.AddRange(rootAllowed);
                        }
                        
                        allowedLayouts[rootArea] = allowed;
                        
                        continue;
                    }

                    var area = new BlockGridConfiguration.BlockGridAreaConfiguration
                    {
                        Alias = $"area{index}",
                        ColumnSpan = section.Grid,
                    };                    

                    var alias = GetBlockGridAreaConfigurationAlias(_shortStringHelper, $"section_{template.Name}_{area.Alias}");
                    area.Key = GetGuidFromAlias(alias);
                    
                    areas.Add(area);

                    if (allowed.Any())
                    {
                        allowedLayouts[area] = allowed;
                    }
                }

                if (!areas.Any())
                {
                    continue;
                }

                if (result.GridColumns == template.Sections.Sum(x => x.Grid))
                {
                    var allowed = new List<string>();
                    
                    // full width
                    foreach (var area in areas)
                    {
                        if (allowedLayouts.TryGetValue(area, out var areaAllowed))
                        {
                            allowed.AddRange(areaAllowed);
                        }
                    }

                    if (allowedLayouts.TryGetValue(rootArea, out var rootAllowed))
                    {
                        allowed.AddRange(rootAllowed);
                    }

                    allowedLayouts[rootArea] = rootAllowed;
                    
                    continue;
                }

                var contentTypeAlias = GetBlockGridLayoutContentTypeAlias(_shortStringHelper, $"section_{template.Name}");

                var layoutBlock = new BlockGridConfiguration.BlockGridBlockConfiguration
                {
                    Label = template?.Name,
                    Areas = areas.ToArray(),
                    AllowAtRoot = true,
                    ContentElementTypeKey = context.GetContentTypeKey(contentTypeAlias),
                    GroupKey = layoutsGroup.Key.ToString(),
                };

                if (layoutBlock.ContentElementTypeKey == Guid.Empty)
                {
                    layoutBlock.ContentElementTypeKey = GetGuidFromAlias(contentTypeAlias);
                    context.AddContentTypeKey(contentTypeAlias, layoutBlock.ContentElementTypeKey);
                }                
                
                layoutBlocks.TryAdd(contentTypeAlias, layoutBlock);
                
                context.AddAdditionalContentType(layoutBlock.ContentElementTypeKey, template?.Name);
            }
        }

        var contentBlocks = new List<BlockGridConfiguration.BlockGridBlockConfiguration>();

        var referencedGridEditors = new List<string>();
        
        // layouts are called rows in UI
        // Rows are predefined cells arranged horizontally
        if (gridConfiguration.Items?.TryGetValue("layouts", out var layouts) == true)
        {
            /*
  "layouts": [
    {
      "label": "",
      "name": "Full width",
      "areas": [
        {
          "grid": 12,
          "editors": [
            "headline"
          ],
          "allowAll": false,
          "allowed": [
            "newslist",
            "layeredContentBlock",
            "interactiveMetroMap",
            "instagram",
            "contentCard",
            "contentBlock",
            "quickAccess",
            "rte",
            "gridMediaFullWidth",
            "newsletter",
            "html",
            "form",
            "countDown",
            "table",
            "timeline"
          ]
        }
      ]
    },
             */

            /*
            var contentTypeHandler = _migrationHandlers.Handlers
                .Where(x => x.SourceVersion == context.SourceVersion)
                .Single(x => x.ItemType == typeof(ContentType).Name);

            contentTypeHandler.DoMigration()
            */

            var gridLayoutConfigurations = layouts
                .ToObject<IEnumerable<GridLayoutConfiguration>>() ?? Enumerable.Empty<GridLayoutConfiguration>();
            
            foreach (var layout in gridLayoutConfigurations)
            {
                if (layout.Areas == null)
                {
                    continue;
                }

                var rowAreas = new List<BlockGridConfiguration.BlockGridAreaConfiguration>();

                foreach (var (gridArea, gridAreaIndex) in layout.Areas.Select((x,i)=>(x,i)))
                {
                    var allowed = new List<string>();

                    if (gridArea.Allowed?.Any() == true)
                    {
                        allowed.AddRange(gridArea.Allowed);
                    }
                    else
                    {
                        referencedGridEditors.Add("*");
                    }

                    referencedGridEditors.AddRange(allowed);

                    if (gridArea.Grid == result.GridColumns)
                    {
                        if (allowed.Any())
                        {
                            if (allowedEditors.TryGetValue(rootArea, out var rootAreaAllowedEditors))
                            {
                                allowed.AddRange(rootAreaAllowedEditors);
                            }

                            allowedEditors[rootArea] = allowed.Distinct();
                        }

                        continue;
                    }

                    
                    var area = new BlockGridConfiguration.BlockGridAreaConfiguration
                    {
                        Alias = $"area{gridAreaIndex}",
                        ColumnSpan = gridArea.Grid,
                        RowSpan = 1,
                    };

                    var alias = GetBlockGridAreaConfigurationAlias(_shortStringHelper, $"layout_{layout.Name}_{area.Alias}");
                    area.Key = GetGuidFromAlias(alias);

                    rowAreas.Add(area);

                    if (allowed.Any())
                    {
                        allowedEditors[area] = allowed;
                    }
                }

                if (!rowAreas.Any())
                {
                    continue;
                }

                var contentTypeAlias = GetBlockGridLayoutContentTypeAlias(_shortStringHelper, layout.Name);

                var layoutBlock = new BlockGridConfiguration.BlockGridBlockConfiguration
                {
                    Label = layout?.Name,
                    Areas = rowAreas.ToArray(),
                    ContentElementTypeKey = context.GetContentTypeKey(contentTypeAlias),
                    GroupKey = layoutsGroup.Key.ToString(),
                };

                if (layoutBlock.ContentElementTypeKey == Guid.Empty)
                {
                    layoutBlock.ContentElementTypeKey = GetGuidFromAlias(contentTypeAlias);
                    context.AddContentTypeKey(contentTypeAlias, layoutBlock.ContentElementTypeKey);
                }
                
                layoutBlocks.TryAdd(contentTypeAlias, layoutBlock);
                
                context.AddAdditionalContentType(layoutBlock.ContentElementTypeKey, layout?.Name);
            }
        }

        foreach (var (alias, block) in layoutBlocks)
        {
            if (allowedLayouts.TryGetValue(rootArea, out var rootAllowed))
            {
                if (rootAllowed.Contains(block.Label))
                {
                    block.AllowAtRoot = true;
                }
            }

            foreach (var area in block.Areas)
            {
                if (!allowedLayouts.TryGetValue(area, out var areaAllowed))
                {
                    continue;
                }

                var layoutContentTypeAliases = areaAllowed.Select(x =>
                    GetBlockGridLayoutContentTypeAlias(_shortStringHelper, x));

                var specifiedAllowance =
                    new List<BlockGridConfiguration.BlockGridAreaConfigurationSpecifiedAllowance>(area.SpecifiedAllowance);
                    
                foreach (var layoutContentTypeAlias in layoutContentTypeAliases)
                {
                    var contentTypeKey = context.GetContentTypeKey(layoutContentTypeAlias);

                    /*
                        var groupKey = layoutBlocks.Values
                            .Where(x => x.ContentElementTypeKey == contentTypeKey && x.GroupKey != null)
                            .Select(x => x.GroupKey == null ? Guid.Empty : new Guid(x.GroupKey))
                            .Distinct()
                            .DefaultIfEmpty()
                            .FirstOrDefault()
                        */

                    if (contentTypeKey != Guid.Empty)
                    {
                        specifiedAllowance.Add(new BlockGridConfiguration.BlockGridAreaConfigurationSpecifiedAllowance
                        {
                            ElementTypeKey = contentTypeKey,
                        });
                    }
                }

                area.SpecifiedAllowance = specifiedAllowance.ToArray();
            }
            
            if (block.ContentElementTypeKey == Guid.Empty)
            {
                block.ContentElementTypeKey = GetGuidFromAlias(alias);
                context.AddContentTypeKey(alias, block.ContentElementTypeKey);
            }
        }
        
        context.AddElementTypes(layoutBlocks.Values
            .Select(x => x.ContentElementTypeKey), true);
        
        var gridEditorToBlocks = new Dictionary<string, IEnumerable<Guid>>(StringComparer.InvariantCultureIgnoreCase);

        foreach (var editor in _gridConfig.EditorsConfig.Editors
                     .Where(x => referencedGridEditors.Contains("*") || referencedGridEditors.Contains(x.Alias, StringComparer.InvariantCultureIgnoreCase)))
        {
            var blocks = MigrateGridEditorToBlocks(editor, gridConfiguration, context).ToList();

            if (!blocks.Any())
            {
                continue;
            }

            gridEditorToBlocks[editor.Alias] = blocks.Select(x => x.ContentElementTypeKey);
            
            contentBlocks.AddRange(blocks);
        }
        
        foreach (var (area, gridEditorAliases) in allowedEditors)
        {
            var allowedElementTypes = new List<Guid>();
            
            foreach (var gridEditorAlias in gridEditorAliases)
            {
                if (gridEditorToBlocks.TryGetValue(gridEditorAlias, out var contentTypeKeys))
                {
                    allowedElementTypes.AddRange(contentTypeKeys);
                }
            }

            var allowedBlocks = contentBlocks
                .Where(x => allowedElementTypes.Contains(x.ContentElementTypeKey))
                .ToArray();

            if (!allowedBlocks.Any())
            {
                continue;
            }
            
            if (area == rootArea)
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

        result.Blocks = contentBlocks
            .Union(layoutBlocks.Values)
            .Where(x => x.ContentElementTypeKey != Guid.Empty)
            .ToArray();

        var referencedGroupKeys = result.Blocks
            .Select(x => x.GroupKey).Distinct().WhereNotNull()
            .Select(x => Guid.TryParse(x, out var k) ? k : Guid.Empty)
            .Distinct()
            .ToArray();
        
        result.BlockGroups = blockGroups
            .Where(x => referencedGroupKeys.Contains(x.Key))
            .ToArray();

        context.AddElementTypes(result.Blocks.Select(x => x.ContentElementTypeKey), true);

        return result;
    }

    protected IEnumerable<BlockGridConfiguration.BlockGridBlockConfiguration> MigrateGridEditorToBlocks(
        IGridEditorConfig editor, GridConfiguration? grid, SyncMigrationContext context)
    {
        if (editor?.Config.TryGetValue("allowedDocTypes", out var allowedDocTypesValue) == true &&
            allowedDocTypesValue is JArray allowedDocTypes)
        {
            /*
            this is a Doc Type Grid Editor. Example of config:
            {
                "name": "Content Block",
                "alias": "contentBlock",
                "view": "/App_Plugins/DocTypeGridEditor/Views/doctypegrideditor.html",
                "render": "/App_Plugins/DocTypeGridEditor/Render/DocTypeGridEditor.cshtml",
                "icon": "icon-umb-content color-red",
                "config": {
                    "allowedDocTypes": ["^gridContentBlock$"],
                    "nameTemplate": "",
                    "enablePreview": true,
                    "viewPath": "/Views/Partials/Grid/Editors/DocTypeGridEditor/",
                    "previewViewPath": "/Views/Partials/Grid/Editors/DocTypeGridEditor/Previews/",
                    "previewCssFilePath": "",
                    "previewJsFilePath": ""
                }
            }
            */

            var allowedDocTypeExpressions = allowedDocTypes.Values<string>().ToArray();

            if (allowedDocTypeExpressions?.Any() == false)
            {
                yield break;
            }

            var contentTypeAliases = context.GetContentTypeAliases();
            
            var allowedContentTypeAliases = contentTypeAliases
                .Where(x => allowedDocTypeExpressions?.WhereNotNull()
                    .Any(y => Regex.IsMatch(x, y, RegexOptions.IgnoreCase)) == true)
                .ToArray();

            if (allowedContentTypeAliases.Any() == false)
            {
                yield break;
            }
            
            var nameTemplate = string.Empty;
            
            if (editor?.Config.TryGetValue("nameTemplate", out var nameTemplateValue) == true)
            {
                nameTemplate = nameTemplateValue as string;
            }
            
            foreach (var allowedContentTypeAlias in allowedContentTypeAliases)
            {
                var elementTypeKey = context.GetContentTypeKey(allowedContentTypeAlias);

                context.AddElementTypes(new[] { elementTypeKey }, true);

                var block = new BlockGridConfiguration.BlockGridBlockConfiguration
                {
                    Label = nameTemplate,
                    ContentElementTypeKey = elementTypeKey,
                };

                yield return block;
            }
        }
        
        /*
        Example of config:
        {
            "name": "Rich text editor",
            "alias": "rte",
            "view": "rte",
            "icon": "icon-article"
        },
        */
    }

    public override string GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (string.IsNullOrWhiteSpace(contentProperty.Value))
        {
            return string.Empty;
        }

        var source = JsonConvert.DeserializeObject<GridValue>(contentProperty.Value)!;

        var blockValue = ConvertToBlockValue(source, context);

        if (blockValue == null)
        {
            return string.Empty;
        }
        
        var value = JsonConvert.SerializeObject(blockValue, Formatting.Indented);

        return value;
    }

    private BlockValue? ConvertToBlockValue(GridValue source, SyncMigrationContext context)
    {
        if (source.Sections.Any() != true)
        {
            // the grid value is empty
            return null!;
        }

        var layoutItems = new List<BlockGridLayoutItem>();
        var contentData = new List<BlockItemData>();
        var settingsData = new List<BlockItemData>();

        var sectionContentTypeAlias = GetBlockGridLayoutContentTypeAlias( _shortStringHelper, $"section_{source.Name}");

        var sections = source.Sections
            .Select(x => (Grid: int.TryParse(x.Grid, out var grid) ? grid : 0, x.Rows))
            .ToArray();

        var gridColumns = sections.Sum(x => x.Grid);
        
        foreach (var (sectionColumns, rows) in sections)
        {
            var sectionIsFullWidth = (sectionColumns == gridColumns);
            
            foreach (var row in rows)
            {
                var areas = row.Areas
                    .Select((x,i) => (Index: i, Grid: int.TryParse(x.Grid, out var grid) ? grid : 0, x.Styles, x.Config, x.Controls))
                    .ToArray();

                var rowColumns = areas.Sum(x => x.Grid);

                var rowIsFullWidth = sectionIsFullWidth && rowColumns == gridColumns;

                var rowLayoutAreas = new List<BlockGridLayoutAreaItem>();
                
                foreach (var (areaIndex, areaColumns, styles, config, controls) in areas)
                {
                    var areaIsFullWidth = rowIsFullWidth && areaColumns == gridColumns;

                    var items = new List<(BlockItemData content, BlockItemData? settings)>();

                    foreach (var control in controls)
                    {
                        var content = GetBlockItemDataFromGridControl(control, context);

                        if (content == null)
                        {
                            continue;
                        }

                        BlockItemData? settings = null;

                        if (control.Config != null || control.Styles != null)
                        {
                            // TODO : map to settings...
                        }

                        items.Add((content, settings));
                    }

                    if (!items.Any())
                    {
                        continue;
                    }

                    var layouts = new List<BlockGridLayoutItem>();

                    foreach (var (content, settings) in items)
                    {
                        contentData.Add(content);

                        var layout = new BlockGridLayoutItem()
                        {
                            ContentUdi = content.Udi,
                            ColumnSpan = areaColumns,
                            RowSpan = 1,
                        };
                        
                        if (settings != null)
                        {
                            layout.SettingsUdi = settings.Udi;
                            settingsData.Add(settings);
                        }
                        
                        layouts.Add(layout);
                    }

                    if (!layouts.Any())
                    {
                        continue;
                    }
                    
                    if (areaIsFullWidth)
                    {
                        layoutItems.AddRange(layouts);
                        continue;
                    }

                    var areaAlias = GetBlockGridAreaConfigurationAlias(_shortStringHelper, $"layout_{row.Name}_area{areaIndex}");
                    
                    var areaItem = new BlockGridLayoutAreaItem
                    {
                        Key = GetGuidFromAlias(areaAlias),
                        Items = layouts.ToArray(),
                    };

                    rowLayoutAreas.Add(areaItem);
                }

                if (rowLayoutAreas.Any())
                {
                    var rowLayoutContentTypeAlias = GetBlockGridLayoutContentTypeAlias(_shortStringHelper, row.Name!);

                    var rowContent = new BlockItemData
                    {
                        Udi = Udi.Create(UmbConstants.UdiEntityType.Element, row.Id),
                        ContentTypeKey = GetGuidFromAlias(rowLayoutContentTypeAlias),
                        ContentTypeAlias = rowLayoutContentTypeAlias,
                    };

                    BlockItemData? rowSettings = null;
                    if (row.Styles != null || row.Config != null)
                    {
                        // TODO : apply settings
                    }

                    var rowLayout = new BlockGridLayoutItem
                    {
                        ContentUdi = rowContent.Udi,
                        SettingsUdi = rowSettings?.Udi,
                        Areas = rowLayoutAreas.ToArray(),
                        ColumnSpan = rowColumns,
                        RowSpan = 1,
                    };

                    layoutItems.Add(rowLayout);

                    contentData.Add(rowContent);
                    if (rowSettings != null)
                    {
                        settingsData.Add(rowSettings);
                    }
                }
            }
        }

        if (!layoutItems.Any())
        {
            return null!;
        }

        ValidateLayouts(layoutItems, contentData);

        var target = new BlockValue
        {
        };
        
        target.ContentData.AddRange(contentData);
        target.SettingsData.AddRange(settingsData);
        
        target.Layout = new Dictionary<string, JToken>
            { { UmbConstants.PropertyEditors.Aliases.BlockGrid, JToken.FromObject(layoutItems) } };

        return target;
    }

    private void ValidateLayouts(IList<BlockGridLayoutItem> layouts, IList<BlockItemData> contentData, int level = 0)
    {
        foreach (var layout in layouts)
        {
            if (contentData.All(x => x.Udi != layout.ContentUdi))
            {
                throw new Exception($"missing content for {layout.ContentUdi} at level {level}");
            }

            foreach (var area in layout.Areas)
            {
                ValidateLayouts(area.Items, contentData, level + 1);
            }
        }
    }

    private BlockItemData? GetBlockItemDataFromGridControl(GridValue.GridControl control, SyncMigrationContext context)
    {
        if (control.Editor.Alias == "rte")
        {
            // TODO : convert rte
            return null;
        }
        
        if (control.Value == null)
        {
            return null;
        }
        
        var id = control.Value.Value<string>("id")!;

        var data = new BlockItemData
        {
            Udi = Udi.Create(UmbConstants.UdiEntityType.Element, Guid.NewGuid()),
            ContentTypeAlias = control.Value.Value<string>("dtgeContentTypeAlias"),
        };

        if (string.IsNullOrWhiteSpace(data.ContentTypeAlias))
        {
            return null!;
        }

        data.ContentTypeKey = context.GetContentTypeKey(data.ContentTypeAlias);
        if (data.ContentTypeKey == Guid.Empty)
        {
            return null;
        }
        
        var elementValue = control.Value.Value<JObject>("value")?.ToObject<IDictionary<string, object?>>();

        if (elementValue == null)
        {
            return null!;
        }

        foreach (var (propertyAlias, value) in elementValue)
        {
            var editorAlias = context.GetEditorAlias(data.ContentTypeAlias, propertyAlias);

            if (editorAlias == null)
            {
                continue;
            }

            var migrator = context.TryGetMigrator(editorAlias.OriginalEditorAlias);
            if (migrator == null)
            {
                continue;
            }

            var childProperty =
                new SyncMigrationContentProperty(editorAlias.OriginalEditorAlias, value?.ToString() ?? string.Empty);

            data.RawPropertyValues[propertyAlias] = migrator.GetContentValue(childProperty, context);
        }

        return data;
    }

    private class GridTemplateConfiguration
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("sections")]
        public IEnumerable<GridSectionConfiguration>? Sections { get; set; }
    }

    private class GridSectionConfiguration
    {
        [JsonProperty("grid")]
        public int Grid { get; set; }

        [JsonProperty("allowAll")]
        public bool? AllowAll { get; set; }
        
        [JsonProperty("allowed")]
        public string[]? Allowed { get; set; }
    }
    
    private class GridLayoutConfiguration
    {
        [JsonProperty("label")]
        public string? Label { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("areas")]
        public IEnumerable<GridAreaConfiguration>? Areas { get; set; }
    }

    private class GridAreaConfiguration
    {
        [JsonProperty("grid")]
        public int Grid { get; set; }

        [JsonProperty("allowAll")]
        public bool? AllowAll { get; set; }
        
        [JsonProperty("allowed")]
        public string[]? Allowed { get; set; }        
    }

}