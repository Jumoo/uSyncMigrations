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
                uSyncMigrations.PluginFolder + "/migration.helpers.js",
                uSyncMigrations.PluginFolder + "/backoffice/uSyncMigrations/dashboard.controller.js",
                uSyncMigrations.PluginFolder + "/migrate.controller.js",
                uSyncMigrations.PluginFolder + "/dialogs/handlerPicker.controller.js",
                uSyncMigrations.PluginFolder + "/components/migrationResults.component.js",
                uSyncMigrations.PluginFolder + "/components/migrationList.component.js",
                uSyncMigrations.PluginFolder + "/components/migrationStatus.component.js",
                uSyncMigrations.PluginFolder + "/components/migrationMessages.component.js",
                uSyncMigrations.PluginFolder + "/components/migrationImport.component.js",
                uSyncMigrations.PluginFolder + "/dialogs/filePicker.controller.js",
                uSyncMigrations.PluginFolder + "/dialogs/upload.controller.js",
            },
            Stylesheets = new[]
            {
                uSyncMigrations.PluginFolder + "/migrations.css"
            }
        });
    }
}
