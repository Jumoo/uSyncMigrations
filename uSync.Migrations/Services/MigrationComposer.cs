using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

using uSync.Migrations.Controllers;
using uSync.Migrations.Handlers;
using uSync.Migrations.Migrators;
using uSync.Migrations.Migrators.Content;
using uSync.Migrations.Migrators.DataTypes;

namespace uSync.Migrations.Services;
public class MigrationComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<DataTypeMigrationCollectionBuilder>()
            .Add(() => builder.TypeLoader.GetTypes<ISyncDataTypeMigrator>());

        builder.WithCollectionBuilder<ContentPropertyMigrationCollectionBuilder>()
            .Add(() => builder.TypeLoader.GetTypes<ISyncContentPropertyMigrator>());

        builder.Services.AddTransient<MigrationFileService>();

        builder.WithCollectionBuilder<MigrationHandlerCollectionBuilder>()
            .Add(() => builder.TypeLoader.GetTypes<ISyncMigrationHandler>());

        builder.Services.AddTransient<MigrationService>();

        builder.AddNotificationHandler<ServerVariablesParsingNotification, MigrationServerVeriablesParser>();

        if (!builder.ManifestFilters().Has<MigrationManifestFilter>())
            builder.ManifestFilters().Append<MigrationManifestFilter>();
    }
}

internal class MigrationManifestFilter : IManifestFilter
{
    public void Filter(List<PackageManifest> manifests)
    {
        manifests.Add(new PackageManifest
        {
            PackageName = uSyncMigrations.AppName,
            BundleOptions = BundleOptions.Independent,
            Version = uSyncMigrations.AppVersion,
            Scripts = new[]
            {
                uSyncMigrations.PluginFolder + "/migration.service.js",
                uSyncMigrations.PluginFolder + "/dashboard.controller.js"
            }
        });
    }
}

internal class MigrationServerVeriablesParser : INotificationHandler<ServerVariablesParsingNotification>
{
    private readonly LinkGenerator _linkGenerator;

    public MigrationServerVeriablesParser(LinkGenerator linkGenerator)
    {
        _linkGenerator = linkGenerator;
    }

    public void Handle(ServerVariablesParsingNotification notification)
    {
        notification.ServerVariables.Add("uSyncMigrations", new Dictionary<string, object>
        {
            { "migrationService",  _linkGenerator.GetUmbracoApiServiceBaseUrl<uSyncMigrationsController>(x => x.GetApi()) ?? "/umbraco/backoffice/api/usyncmigrations/" }
        });
    }
}
