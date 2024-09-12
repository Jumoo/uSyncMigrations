﻿using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

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

        // Take only the settings that have applyTo = row. Other value here could be cell.
        // TODO: Implement cell settings converter.
        var gridSettings = gridConfig.Concat(gridStyles).Where(s => s.ApplyTo != "cell");

        AddGridLayoutSettings(gridSettings, gridBlockContext, context, gridAlias);
    }

    private IEnumerable<GridSettingsConfigurationItem> GetGridSettingsFromConfig(JToken? config)
    {
        if (config == null)
        {
            return Enumerable.Empty<GridSettingsConfigurationItem>();
        }

        return config.ToObject<IEnumerable<GridSettingsConfigurationItem>>() ?? Enumerable.Empty<GridSettingsConfigurationItem>();
    }

    private void AddGridLayoutSettings(IEnumerable<GridSettingsConfigurationItem> gridLayoutConfigurations, GridToBlockGridConfigContext gridBlockContext, SyncMigrationContext context, string gridAlias)
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

            var additionalDataType = gridSettingPropertyMigrator?.GetAdditionalDataType(dataTypeAlias, configItem.Prevalues?.Select(v => v.Label!));
            if (additionalDataType != null)
            {
                context.DataTypes.AddNewDataType(additionalDataType);
            }

            return new NewContentTypeProperty(configItem.Label ?? contentTypeAlias, contentTypeAlias, dataTypeAlias, orginalEditorAlias: null, configItem.Description);
        }).WhereNotNull();

        var alias = _conventions.LayoutSettingsContentTypeAlias(gridAlias);

        context.ContentTypes.AddNewContentType(new NewContentTypeInfo(alias.ToGuid(), alias, alias, "icon-book color-red", "BlockGrid/Settings")
        {
            Description = alias,
            IsElement = true,
            Properties = contentTypeProperties.ToList()
        });
    }
}