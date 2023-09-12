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
            AddGridConfig(gridBlockContext.GridConfiguration.GetItemBlock("config"), gridBlockContext, context, gridAlias);
        }

        private void AddGridConfig(JToken? config, GridToBlockGridConfigContext gridBlockContext, SyncMigrationContext context, string gridAlias)
        {
            if (config == null) return;

            var gridLayoutConfigurations = config
                .ToObject<IEnumerable<GridConfigConfigurationItem>>() ?? Enumerable.Empty<GridConfigConfigurationItem>();

            var contentTypeProperties = gridLayoutConfigurations.Select(configItem =>
            {
                var contentType = configItem.Key;

                return new NewContentTypeProperty()
                {
                    Name = configItem.Label,
                    Alias = configItem.Key,
                    DataTypeAlias = configItem.View,
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

            

/*            context.
*/            /*            context.ContentTypes.AddNewContentType(new NewContentTypeInfo
                        {
                            Key = contentTypeAlias.ToGuid(),
                            Alias = contentTypeAlias,
                            Name = configItem.Label ?? contentTypeAlias,
                            Description = "Grid Settings",
                            Folder = "BlockGrid/Settings",
                            Icon = "icon-cog color-purple",
                            IsElement = true
                        });*/
        }

        private void AddGridStyles(JToken? config, GridToBlockGridConfigContext gridBlockContext, SyncMigrationContext context)
        {
            throw new NotImplementedException();
        }
    }
}
