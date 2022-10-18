using Umbraco.Cms.Core.DependencyInjection;

namespace uSync.Migrations.Composing;

public static class UmbracoBuilderExtensions
{
    public static SyncPropertyMigratorCollectionBuilder SyncPropertyMigrators(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<SyncPropertyMigratorCollectionBuilder>();

    public static SyncMigrationHandlerCollectionBuilder SyncMigrationHandlers(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<SyncMigrationHandlerCollectionBuilder>();
}
