using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

using uSync.BackOffice;
using uSync.Migrations.Migrators.Grid.Config.Migrators;
using uSync.Migrations.Migrators.Grid.Content.BlockMigrators;
using uSync.Migrations.Migrators.Grid.Helpers;
using uSync.Migrations.Migrators.NestedContent;

namespace uSync.Migrations.Migrators;

public static class MigratorsBuilderExtensions
{
    public static IUmbracoBuilder AddSyncMigrators(this IUmbracoBuilder builder)
    {
        // grid things. 
        builder.Services.AddTransient<ISyncGridNameService, SyncGridNameService>();
        builder.Services.AddTransient<ISyncGridContentTypeFinder, SyncGridContentTypeFinder>();

        builder.WithCollectionBuilder<GridSettingsViewMigratorCollectionBuilder>()
            .Add(() => builder.TypeLoader.GetTypes<IGridSettingsViewMigrator>());

        builder.WithCollectionBuilder<SyncBlockMigratorCollectionBuilder>()
            .Add(() => builder.TypeLoader.GetTypes<ISyncBlockMigrator>());

        // nested content doc types become element types for block list.
        builder.AddNotificationAsyncHandler<uSyncImportingItemNotification, NestedContentElementTypeImportingHandler>();

        return builder;
    }
}
