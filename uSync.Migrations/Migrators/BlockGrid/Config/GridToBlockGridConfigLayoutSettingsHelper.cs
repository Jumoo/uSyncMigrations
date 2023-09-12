using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Umbraco.Extensions;
using uSync.Migrations.Context;
using uSync.Migrations.Migrators.BlockGrid.Extensions;
using uSync.Migrations.Migrators.BlockGrid.Models;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.BlockGrid.Config
{
    internal class GridToBlockGridConfigLayoutSettingsHelper
    {
        private readonly GridConventions _conventions;

        public GridToBlockGridConfigLayoutSettingsHelper(GridConventions conventions,
            ILogger<GridToBlockGridConfigLayoutBlockHelper> logger)
        {
            _conventions = conventions;
        }

        public void AddGridSettings(GridToBlockGridConfigContext gridBlockContext, SyncMigrationContext context, string gridAlias)
        {
            var gridConfig = GetGridSettingsFromConfig(gridBlockContext.GridConfiguration?.GetItemBlock("config"));

            var gridStyles = GetGridSettingsFromConfig(gridBlockContext.GridConfiguration?.GetItemBlock("styles"));

            // Take only the settings that have applyTo = row. Other value here could be cell.
            var gridSettings = gridConfig.Concat(gridStyles).Where(s => s.ApplyTo == "row");

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
                var contentTypeAlias = configItem.Key!;

                return new NewContentTypeProperty()
                {
                    Name = configItem.Label ?? contentTypeAlias,
                    Alias = contentTypeAlias,
                    DataTypeAlias = configItem?.View,
                };
            });

            var alias = _conventions.LayoutSettingsContentTypeAlias(gridAlias);

            context.ContentTypes.AddNewContentType(new NewContentTypeInfo
            {
                Name = alias,
                Alias = alias,
                Key = alias.ToGuid(),
                Description = alias,
                Icon = "icon-book color-red",
                IsElement = true,
                Folder = "BlockGrid/Settings",
                Properties = contentTypeProperties.ToList()
            });
        }
    }
}
