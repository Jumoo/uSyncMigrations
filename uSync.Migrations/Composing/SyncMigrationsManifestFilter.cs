using Umbraco.Cms.Core.Manifest;

namespace uSync.Migrations.Composing;

internal class SyncMigrationsManifestFilter : IManifestFilter
{
    public void Filter(List<PackageManifest> manifests)
    {
        manifests.Add(new()
        {
            PackageName = uSyncMigrations.AppName,
            BundleOptions = BundleOptions.Independent,
            Version = uSyncMigrations.AppVersion,
            Scripts = new[]
            {
                uSyncMigrations.PluginFolder + "/migration.service.js",
                uSyncMigrations.PluginFolder + "/dashboard.controller.js",
                uSyncMigrations.PluginFolder + "/migrate.controller.js",
                uSyncMigrations.PluginFolder + "/dialogs/handlerPicker.controller.js",
                uSyncMigrations.PluginFolder + "/components/migrationResults.component.js"
            },
            Stylesheets = new[]
            {
                uSyncMigrations.PluginFolder + "/migrations.css"
            }
        });
    }
}
