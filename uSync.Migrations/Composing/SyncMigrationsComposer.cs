using Microsoft.Extensions.DependencyInjection;

using NUglify.JavaScript.Syntax;

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

using uSync.Migrations.Configuration;
using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Handlers;
using uSync.Migrations.Migrators;
using uSync.Migrations.Notifications;
using uSync.Migrations.Services;

namespace uSync.Migrations.Composing;

public class SyncMigrationsComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AdduSyncMigrations();
    }
}

public static class SyncMigrationsBuilderExtensions
{
    /// <summary>
    ///  Add uSync Migrations to your project
    /// </summary>
    /// <remarks>
    ///  if your startup.cs has an .AddComposers() line then this will be automatically added anyway
    /// </remarks>
    public static IUmbracoBuilder AdduSyncMigrations(this IUmbracoBuilder builder)
    {
        // stop a double add. 
        if (builder.Services.Any(x => x.ServiceType == typeof(SyncMigrationFileService)))
        {
            return builder;
        }

        builder
            .WithCollectionBuilder<SyncPropertyMigratorCollectionBuilder>()
                .Append(builder.TypeLoader.GetTypes<ISyncPropertyMigrator>());

        builder.Services.AddTransient<SyncMigrationFileService>();

        builder
            .WithCollectionBuilder<SyncMigrationHandlerCollectionBuilder>()
                .Add(() => builder.TypeLoader.GetTypes<ISyncMigrationHandler>());

        builder
            .WithCollectionBuilder<SyncMigrationProfileCollectionBuilder>()
                .Add(() => builder.TypeLoader.GetTypes<ISyncMigrationProfile>());

        builder.Services.AddTransient<SyncMigrationService>();
        builder.Services.AddTransient<SyncMigrationConfigurationService>();

        builder.AddNotificationHandler<ServerVariablesParsingNotification, SyncMigrationsServerVariablesParsingNotificationHandler>();

        if (builder.ManifestFilters().Has<SyncMigrationsManifestFilter>() == false)
        {
            builder.ManifestFilters().Append<SyncMigrationsManifestFilter>();
        }

        return builder;
    }
}