using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

using uSync.Migrations.Core.Configuration;
using uSync.Migrations.Core.Handlers;
using uSync.Migrations.Core.Legacy.Grid;
using uSync.Migrations.Core.Migrators;
using uSync.Migrations.Core.Plans;
using uSync.Migrations.Core.Services;
using uSync.Migrations.Core.Validation;

namespace uSync.Migrations.Core;

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
        if (builder.Services.Any(x => x.ServiceType == typeof(ISyncMigrationFileService)))
        {
            return builder;
        }

        builder.Services.AddSingleton<ILegacyGridConfig, LegacyGridConfig>();

        builder
            .WithCollectionBuilder<SyncPropertyMigratorCollectionBuilder>()
                .Append(builder.TypeLoader.GetTypes<ISyncPropertyMigrator>());

        builder
            .WithCollectionBuilder<SyncPropertyMergingCollectionBuilder>()
                .Append(builder.TypeLoader.GetTypes<ISyncPropertyMergingMigrator>());

        builder.Services.AddTransient<ISyncMigrationFileService, SyncMigrationFileService>();

        builder
            .WithCollectionBuilder<SyncMigrationHandlerCollectionBuilder>()
                .Add(() => builder.TypeLoader.GetTypes<ISyncMigrationHandler>());

        builder
            .WithCollectionBuilder<SyncMigrationProfileCollectionBuilder>()
                .Add(builder.TypeLoader.GetTypes<ISyncMigrationPlan>());

        builder
            .WithCollectionBuilder<SyncMigrationValidatorCollectionBuilder>()
                .Add(() => builder.TypeLoader.GetTypes<ISyncMigrationValidator>());

        builder.Services.AddTransient<ISyncMigrationStatusService, SyncMigrationStatusService>();
        builder.Services.AddTransient<ISyncMigrationService, SyncMigrationService>();
        builder.Services.AddTransient<ISyncMigrationConfigurationService, SyncMigrationConfigurationService>();

        builder.Services.AddTransient<ISyncMigrationPackService, SyncMigrationPackService>();

        builder.Services.AddOptions<uSyncMigrationOptions>().Configure<IConfiguration>((settings, configuration)
            => configuration.GetSection(uSyncMigrationOptions.Section).Bind(settings));

        return builder;
    }


    public static SyncPropertyMigratorCollectionBuilder SyncPropertyMigrators(this IUmbracoBuilder builder)
    => builder.WithCollectionBuilder<SyncPropertyMigratorCollectionBuilder>();

    public static SyncMigrationHandlerCollectionBuilder SyncMigrationHandlers(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<SyncMigrationHandlerCollectionBuilder>();

}