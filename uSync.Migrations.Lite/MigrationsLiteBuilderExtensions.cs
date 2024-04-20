using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Notifications;

using uSync.Migrations.Core;
using uSync.Migrations.Lite.Services;

namespace uSync.Migrations.Lite;

/// <summary>
///  Lite Client, is installed with uSync (v13.2), it offers simple 
///  conversion for v8 -> v13 sites. 
/// </summary>
public class MigrationsLiteComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AdduSyncMigrationsLite();
    }
}

public static class MigrationsLiteBuilderExtensions
{
    public static IUmbracoBuilder AdduSyncMigrationsLite(this IUmbracoBuilder builder)
    {
        builder.AdduSyncMigrations();

        builder.Services.AddTransient<ISyncMigrationConversionService, SyncMigrationConversionService>();

        builder.AddNotificationHandler<ServerVariablesParsingNotification, VariablesParserHandler>();
        builder.ManifestFilters().Append<MigrationsLiteManifestFilter>();

        return builder;
    }
}

internal class MigrationsLiteManifestFilter : IManifestFilter
{
    public void Filter(List<PackageManifest> manifests)
    {
        manifests.Add(new PackageManifest
        {
            PackageName = "uSync.Migrations.Lite",
            Scripts = new[]
            {
                "/App_Plugins/uSyncMigrationsLite/dashboard.controller.js",
                "/App_Plugins/uSyncMigrationsLite/migration.service.js"
            }
        });
    }
}
