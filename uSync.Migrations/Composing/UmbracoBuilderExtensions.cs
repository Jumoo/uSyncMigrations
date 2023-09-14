using Umbraco.Cms.Core.DependencyInjection;

using uSync.Migrations.Migrators.BlockGrid.BlockMigrators;
using uSync.Migrations.Migrators.BlockGrid.Config;

namespace uSync.Migrations.Composing;

public static class UmbracoBuilderExtensions
{
    public static SyncPropertyMigratorCollectionBuilder SyncPropertyMigrators(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<SyncPropertyMigratorCollectionBuilder>();

    public static SyncMigrationHandlerCollectionBuilder SyncMigrationHandlers(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<SyncMigrationHandlerCollectionBuilder>();

    public static SyncBlockMigratorCollectionBuilder SyncBlockMigrators(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<SyncBlockMigratorCollectionBuilder>();

    public static GridSettingsViewMigratorCollectionBuilder GridSettingsMigrators(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<GridSettingsViewMigratorCollectionBuilder>();
}
