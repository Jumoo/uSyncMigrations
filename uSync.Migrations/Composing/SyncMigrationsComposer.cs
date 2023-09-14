using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Web.BackOffice.Authorization;

using uSync.Migrations.Configuration;
using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Handlers;
using uSync.Migrations.Legacy.Grid;
using uSync.Migrations.Migrators;
using uSync.Migrations.Migrators.BlockGrid.BlockMigrators;
using uSync.Migrations.Migrators.BlockGrid.Config;
using uSync.Migrations.Migrators.BlockGrid.Extensions;
using uSync.Migrations.Migrators.Community;
using uSync.Migrations.Migrators.Community.Archetype;
using uSync.Migrations.Notifications;
using uSync.Migrations.Services;
using uSync.Migrations.Validation;

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
        if (builder.Services.Any(x => x.ServiceType == typeof(ISyncMigrationFileService)))
        {
            return builder;
        }

        builder.Services.AddSingleton<GridConventions>();

        builder.Services.AddSingleton<ILegacyGridConfig, LegacyGridConfig>();

        builder
            .WithCollectionBuilder<SyncPropertyMigratorCollectionBuilder>()
                .Append(builder.TypeLoader.GetTypes<ISyncPropertyMigrator>());

        builder
            .WithCollectionBuilder<SyncBlockMigratorCollectionBuilder>()
                .Add(() => builder.TypeLoader.GetTypes<ISyncBlockMigrator>());

        builder
            .WithCollectionBuilder<GridSettingsViewMigratorCollectionBuilder>()
                .Add(() => builder.TypeLoader.GetTypes<IGridSettingsViewMigrator>());

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

        builder
            .WithCollectionBuilder<ArchetypeMigrationConfigurerCollectionBuilder>()
                .Add(() => builder.TypeLoader.GetTypes<IArchetypeMigrationConfigurer>());

        builder.Services.AddAuthorization(o =>
            CreatePolicies(o, Constants.Security.BackOfficeAuthenticationType));

        builder.Services.AddTransient<ISyncMigrationStatusService, SyncMigrationStatusService>();
        builder.Services.AddTransient<ISyncMigrationService, SyncMigrationService>();
        builder.Services.AddTransient<ISyncMigrationConfigurationService, SyncMigrationConfigurationService>();
        builder.Services.AddOptions<NuPickerMigrationOptions>().Configure<IConfiguration>((settings, configuration)
            => configuration.GetSection(NuPickerMigrationOptions.Section).Bind(settings));

        builder.Services.AddOptions<ArchetypeMigrationOptions>().Configure<IConfiguration>((settings, configuration)
            => configuration.GetSection(ArchetypeMigrationOptions.Section).Bind(settings));
        builder.Services.AddOptions<uSyncMigrationOptions>().Configure<IConfiguration>((settings, configuration)
            => configuration.GetSection(uSyncMigrationOptions.Section).Bind(settings));
        builder.AddNotificationHandler<ServerVariablesParsingNotification, SyncMigrationsServerVariablesParsingNotificationHandler>();

        if (builder.ManifestFilters().Has<SyncMigrationsManifestFilter>() == false)
        {
            builder.ManifestFilters().Append<SyncMigrationsManifestFilter>();
        }

        return builder;
    }

    private static void CreatePolicies(AuthorizationOptions options,
        string backOfficeAuthorizationScheme)
    {
        options.AddPolicy(uSyncMigrationsAuthorizationPolicies.MigrationsTreeAccess,
            policy =>
            {
                policy.AuthenticationSchemes.Add(backOfficeAuthorizationScheme);
                policy.Requirements.Add(new TreeRequirement(uSyncMigrations.TreeName));
            });
    }
}