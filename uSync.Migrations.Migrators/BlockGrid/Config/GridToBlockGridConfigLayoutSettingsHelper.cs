using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

using uSync.Migrations.Migrators.BlockGrid.Extensions;
using uSync.Migrations.Migrators.BlockGrid.Models;
using uSync.Migrations.Migrators.BlockGrid.SettingsMigrators;

namespace uSync.Migrations.Migrators.BlockGrid.Config;

internal class GridToBlockGridConfigLayoutSettingsHelper
{
    private readonly GridConventions _conventions;
    private readonly GridSettingsViewMigratorCollection _gridSettingsViewMigrators;
    private readonly ILogger<GridToBlockGridConfigLayoutSettingsHelper> _logger;

    public bool AnyAreaSettings { get; set; } = false;

    public GridToBlockGridConfigLayoutSettingsHelper(
        GridConventions conventions,
        GridSettingsViewMigratorCollection gridSettingsViewMigrators,
        ILogger<GridToBlockGridConfigLayoutSettingsHelper> logger)
    {
        _conventions = conventions;
        _gridSettingsViewMigrators = gridSettingsViewMigrators;
        _logger = logger;
    }

    public void AddGridSettings(GridToBlockGridConfigContext gridBlockContext, SyncMigrationContext context, string gridAlias)
    {
        var gridConfig = GetGridSettingsFromConfig(gridBlockContext.GridConfiguration?.GetItemBlock("config"));

        var gridStyles = GetGridSettingsFromConfig(gridBlockContext.GridConfiguration?.GetItemBlock("styles"));

        var gridRowSettings = gridConfig.Concat(gridStyles).Where(s => s.ApplyTo != "cell");

        AddGridLayoutSettings(gridRowSettings, gridBlockContext, context, gridAlias, isArea: false);

        var gridAreaSettings = gridConfig.Concat(gridStyles).Where(s => s.ApplyTo != "row");

        AnyAreaSettings = gridAreaSettings.Any();

        AddGridLayoutSettings(gridAreaSettings, gridBlockContext, context, gridAlias, isArea: true);
    }

    private IEnumerable<GridSettingsConfigurationItem> GetGridSettingsFromConfig(JToken? config)
    {
        if (config == null)
        {
            return Enumerable.Empty<GridSettingsConfigurationItem>();
        }

        return config.ToObject<IEnumerable<GridSettingsConfigurationItem>>() ?? Enumerable.Empty<GridSettingsConfigurationItem>();
    }

    private void AddGridLayoutSettings(IEnumerable<GridSettingsConfigurationItem> gridLayoutConfigurations, GridToBlockGridConfigContext gridBlockContext, SyncMigrationContext context, string gridAlias, bool isArea = false)
    {
        var contentTypeProperties = gridLayoutConfigurations.Where(configItem => configItem.Key is not null).Select(configItem =>
        {
            var contentTypeAlias = configItem.Key;
            if (contentTypeAlias.IsNullOrWhiteSpace() == true)
            {
                _logger.LogError("No key defined for grid layout configuration in {alias}", gridAlias);
                return null;
            }
            var gridSettingPropertyMigrator = _gridSettingsViewMigrators.GetMigrator(configItem.View);
            string? migratorDataTypeAlias = gridSettingPropertyMigrator?.GetNewDataTypeAlias(gridAlias, configItem.Label);

            var dataTypeAlias = !migratorDataTypeAlias.IsNullOrWhiteSpace()
                                ? migratorDataTypeAlias
                                : configItem.View;
            if (dataTypeAlias.IsNullOrWhiteSpace() == true)
            {
                _logger.LogError("No view defined for grid layout configuration in {alias}", gridAlias);
                return null;
            }

            var additionalDataType = gridSettingPropertyMigrator?.GetAdditionalDataType(dataTypeAlias, configItem.Prevalues);
            if (additionalDataType != null)
            {
                context.DataTypes.AddNewDataType(additionalDataType);
            }

            return new NewContentTypeProperty(configItem.Label ?? contentTypeAlias, contentTypeAlias, dataTypeAlias, orginalEditorAlias: null, configItem.Description);
        }).WhereNotNull();

        var alias = isArea ? _conventions.LayoutAreaSettingsContentTypeAlias(gridAlias) : _conventions.LayoutSettingsContentTypeAlias(gridAlias);

        context.ContentTypes.AddNewContentType(new NewContentTypeInfo(alias.ToGuid(), alias, alias, "icon-book color-red", "BlockGrid/Settings")
        {
            Description = alias,
            IsElement = true,
            Properties = contentTypeProperties.ToList()
        });

        if (isArea)
        {
            var areaSettingsBlock = new BlockGridConfiguration.BlockGridBlockConfiguration
            {
                Label = _conventions.AreaSettingsElementTypeName,
                AllowAtRoot = false,
                ContentElementTypeKey = context.GetContentTypeKeyOrDefault(_conventions.AreaSettingsElementTypeAlias, _conventions.AreaSettingsElementTypeAlias.ToGuid()),
                SettingsElementTypeKey = context.GetContentTypeKeyOrDefault(alias, alias.ToGuid()),
                GroupKey = gridBlockContext.AreaSettingsGroup.Key.ToString(),
                BackgroundColor = Grid.LayoutBlocks.Background,
                IconColor = Grid.LayoutBlocks.Icon,
                ForceHideContentEditorInOverlay = true
            };

            gridBlockContext.AreaSettingsBlocks.Add(areaSettingsBlock);

        }
    }
}