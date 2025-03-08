using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Web.BackOffice.Authorization;
using uSync.Migrations.Client.Notifications;
using uSync.Migrations.Core;

namespace uSync.Migrations.Client;

public class uSyncMigrationsComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AdduSyncMigrations();
        builder.AdduSyncMigrationsUI();
    }
}

internal static class uSyncMigrationsBuilderExtensions
{
    public static IUmbracoBuilder AdduSyncMigrationsUI(this IUmbracoBuilder builder)
    {

        if (builder.ManifestFilters().Has<SyncMigrationsManifestFilter>() == false)
            builder.ManifestFilters().Append<SyncMigrationsManifestFilter>();

        builder.Services.AddAuthorization(o =>
            CreatePolicies(o, UmbConstants.Security.BackOfficeAuthenticationType));


        builder.AddNotificationHandler<ServerVariablesParsingNotification, SyncMigrationsServerVariablesParsingNotificationHandler>();
        return builder;
    }

    private static void CreatePolicies(AuthorizationOptions options, string backOfficeAuthorizationScheme)
    {
        options.AddPolicy(uSyncMigrationsAuthorizationPolicies.MigrationsTreeAccess,
            policy =>
            {
                policy.AuthenticationSchemes.Add(backOfficeAuthorizationScheme);
                policy.Requirements.Add(new TreeRequirement(uSyncMigrationsClient.TreeName));
            });
    }

}

internal class SyncMigrationsManifestFilter : IManifestFilter
{
    public void Filter(List<PackageManifest> manifests)
    {
        manifests.Add(new()
        {
            PackageName = uSyncMigrationsClient.AppName,
            BundleOptions = BundleOptions.Independent,
            Version = uSyncMigrationsClient.AppVersion,
            Scripts = new[]
            {
                uSyncMigrationsClient.PluginFolder + "/migration.service.js",
                uSyncMigrationsClient.PluginFolder + "/migration.helpers.js",
                uSyncMigrationsClient.PluginFolder + "/backoffice/uSyncMigrations/dashboard.controller.js",
                uSyncMigrationsClient.PluginFolder + "/migrate.controller.js",
                uSyncMigrationsClient.PluginFolder + "/dialogs/handlerPicker.controller.js",
                uSyncMigrationsClient.PluginFolder + "/components/migrationResults.component.js",
                uSyncMigrationsClient.PluginFolder + "/components/migrationList.component.js",
                uSyncMigrationsClient.PluginFolder + "/components/migrationStatus.component.js",
                uSyncMigrationsClient.PluginFolder + "/components/migrationMessages.component.js",
                uSyncMigrationsClient.PluginFolder + "/components/migrationImport.component.js",
                uSyncMigrationsClient.PluginFolder + "/dialogs/filePicker.controller.js",
                uSyncMigrationsClient.PluginFolder + "/dialogs/upload.controller.js",
                uSyncMigrationsClient.PluginFolder + "/dialogs/download.controller.js"
            },
            Stylesheets = new[]
            {
                uSyncMigrationsClient.PluginFolder + "/migrations.css"
            }
        });
    }
}
