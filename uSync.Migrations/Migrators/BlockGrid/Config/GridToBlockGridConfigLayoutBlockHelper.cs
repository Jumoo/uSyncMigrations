using System.Data;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;
using uSync.Migrations.Context;
using uSync.Migrations.Migrators.BlockGrid.Extensions;
using uSync.Migrations.Migrators.BlockGrid.Models;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.BlockGrid.Config;


internal class GridToBlockGridConfigLayoutBlockHelper
{
    private readonly GridConventions _conventions;
    private readonly ILogger<GridToBlockGridConfigLayoutBlockHelper> _logger;

    public GridToBlockGridConfigLayoutBlockHelper(
        GridConventions conventions,
        ILogger<GridToBlockGridConfigLayoutBlockHelper> logger)
    {
        _conventions = conventions;
        _logger = logger;
    }

    public void AddLayoutBlocks(GridToBlockGridConfigContext gridBlockContext, SyncMigrationContext context, string dataTypeAlias)
    {
        // gather all the layout blocks we can from the templates 
        // and layouts sections of the config. 
        GetTemplateLayouts(gridBlockContext.GridConfiguration.GetItemBlock("templates"), gridBlockContext, context);

        GetLayoutLayouts(gridBlockContext.GridConfiguration.GetItemBlock("layouts"), gridBlockContext, context, dataTypeAlias);

        AddContentTypesForLayoutBlocks(gridBlockContext, context);
    }

    private void GetTemplateLayouts(JToken? templates, GridToBlockGridConfigContext gridBlockContext, SyncMigrationContext context)
    {
        if (templates == null) return;

        var gridTemplateConfiguration = templates
                .ToObject<IEnumerable<GridTemplateConfiguration>>() ?? Enumerable.Empty<GridTemplateConfiguration>();

        _logger.LogDebug("Processing Template Layouts for grid to blockgrid");

        foreach (var template in gridTemplateConfiguration)
        {
            if (template.Sections == null) continue;

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

                if (section.Grid == gridBlockContext.GridColumns)
                {
                    _logger.LogDebug("Adding [{allowed}] to section", string.Join(",", allowed));
                    gridBlockContext.AppendToRootLayouts(allowed);
                    continue;
                }

                var area = new BlockGridConfiguration.BlockGridAreaConfiguration
                {
                    Alias = $"area_{index}",
                    ColumnSpan = section.Grid,
		    RowSpan = 1
                };

                var alias = _conventions.GridAreaConfigAlias(area.Alias);
                area.Key = alias.ToGuid();

                areas.Add(area);

                if (allowed.Any())
                {
                    gridBlockContext.AllowedLayouts[area] = allowed;
                }
            }

            if (areas.Count == 0)
            {
                _logger.LogDebug("No areas added");
                continue;
            }

            if (gridBlockContext.GridColumns == template.Sections.Sum(x => x.Grid))
            {
                var allowed = new List<string>();
                foreach (var area in areas)
                {
                    allowed.AddRange(gridBlockContext.GetAllowedLayouts(area));
                }

                allowed.AddRange(gridBlockContext.GetRootAllowedLayouts());

                //continue;
            }

            var contentTypeAlias = _conventions.TemplateContentTypeAlias(template.Name);

            var layoutBlock = new BlockGridConfiguration.BlockGridBlockConfiguration
            {
                Label = template?.Name,
                Areas = areas.ToArray(),
                AllowAtRoot = true,
                ContentElementTypeKey = context.GetContentTypeKeyOrDefault(contentTypeAlias, contentTypeAlias.ToGuid()),
                GroupKey = gridBlockContext.LayoutsGroup.Key.ToString(),
                BackgroundColor = Grid.LayoutBlocks.Background,
                IconColor = Grid.LayoutBlocks.Icon
			};

            gridBlockContext.LayoutBlocks.TryAdd(contentTypeAlias, layoutBlock);

            context.ContentTypes.AddNewContentType(new NewContentTypeInfo
            {
                Key = layoutBlock.ContentElementTypeKey,
                Alias = contentTypeAlias,
                Name = template?.Name ?? contentTypeAlias,
                Description = "Grid Layoutblock",
                Folder = "BlockGrid/Layouts",
                Icon = "icon-layout color-purple",
                IsElement = true
            });
        }
    }

    private void GetLayoutLayouts(JToken? layouts, GridToBlockGridConfigContext gridBlockContext, SyncMigrationContext context, string dataTypeAlias)
    {
        if (layouts == null) return;

        var gridLayoutConfigurations = layouts
            .ToObject<IEnumerable<GridLayoutConfiguration>>() ?? Enumerable.Empty<GridLayoutConfiguration>();

        foreach (var layout in gridLayoutConfigurations)
        {
            if (layout.Areas == null) continue;

            var rowAreas = new List<BlockGridConfiguration.BlockGridAreaConfiguration>();

            foreach (var (gridArea, gridAreaIndex) in layout.Areas.Select((x, i) => (x, i)))
            {
                var allowed = new List<string>();

                if (gridArea.Allowed?.Any() == true)
                {
                    allowed.AddRange(gridArea.Allowed);
                }
                else
                {
                    allowed.Add("*");
                }

                if (gridArea.Grid == gridBlockContext.GridColumns)
                {
                    gridBlockContext.AppendToRootEditors(allowed);
                    gridBlockContext.AppendToRootLayouts(allowed);
                    continue;
                }

                var area = new BlockGridConfiguration.BlockGridAreaConfiguration
                {
                    Alias = _conventions.AreaAlias(gridAreaIndex),
                    ColumnSpan = gridArea.Grid,
                    RowSpan = 1
                };

                var alias = _conventions.LayoutAreaAlias(layout.Name, area.Alias);
                area.Key = alias.ToGuid();

                rowAreas.Add(area);

                if (allowed.Any())
                    gridBlockContext.AllowedEditors[area] = allowed;
            }

            if (rowAreas.Count == 0) continue;

            var contentTypeAlias = _conventions.LayoutContentTypeAlias(layout.Name);
            var settingsContentTypeAlias = _conventions.LayoutSettingsContentTypeAlias(dataTypeAlias);

            var layoutBlock = new BlockGridConfiguration.BlockGridBlockConfiguration
            {
                Label = layout?.Name,
                Areas = rowAreas.ToArray(),
                ContentElementTypeKey = context.GetContentTypeKeyOrDefault(contentTypeAlias, contentTypeAlias.ToGuid()),
                SettingsElementTypeKey = context.GetContentTypeKeyOrDefault(settingsContentTypeAlias, settingsContentTypeAlias.ToGuid()),
                GroupKey = gridBlockContext.LayoutsGroup.Key.ToString(),
				BackgroundColor = Grid.LayoutBlocks.Background,
				IconColor = Grid.LayoutBlocks.Icon,
			};

            gridBlockContext.LayoutBlocks.TryAdd(contentTypeAlias, layoutBlock);

            context.ContentTypes.AddNewContentType(new NewContentTypeInfo
            {
                Key = layoutBlock.ContentElementTypeKey,
                Alias = contentTypeAlias,
                Name = layout?.Name ?? contentTypeAlias,
                Description = "Grid Layoutblock",
                Folder = "BlockGrid/Layouts",
                Icon = "icon-layout color-purple",
                IsElement = true
            });
        }

    }

    private void AddContentTypesForLayoutBlocks(GridToBlockGridConfigContext gridBlockContext, SyncMigrationContext context)
    {
        var rootAllowed = gridBlockContext.GetRootAllowedLayouts();

        foreach (var (alias, block) in gridBlockContext.LayoutBlocks)
        {
            if (rootAllowed.Contains(block.Label))
            {
                block.AllowAtRoot = true;
            }

            foreach (var area in block.Areas)
            {
                var areaAllowed = gridBlockContext.GetAllowedLayouts(area);

                if (!areaAllowed.Any()) continue;

                var layoutContentTypeAliases = areaAllowed
                    .Select(x => _conventions.LayoutContentTypeAlias(x));

                var specificedAllowance = new List<BlockGridConfiguration.BlockGridAreaConfigurationSpecifiedAllowance>(area.SpecifiedAllowance);

                foreach (var layoutContentTypeAlias in layoutContentTypeAliases)
                {
                    var contentTypeKey = context.ContentTypes.GetKeyByAlias(layoutContentTypeAlias);
                    if (contentTypeKey != Guid.Empty)
                    {
                        specificedAllowance.Add(new()
                        {
                            ElementTypeKey = contentTypeKey
                        });
                    }
                }

                area.SpecifiedAllowance = specificedAllowance.ToArray();
            }

            if (block.ContentElementTypeKey == Guid.Empty)
            {
                block.ContentElementTypeKey = alias.ToGuid();
                context.ContentTypes.AddAliasAndKey(alias, block.ContentElementTypeKey);
            }

            context.ContentTypes.AddElementType(block.ContentElementTypeKey);
        }
    }
}
