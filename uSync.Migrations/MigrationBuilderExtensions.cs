using Umbraco.Cms.Core.DependencyInjection;

using uSync.Migrations.Handlers;

namespace uSync.Migrations;
public static class MigrationBuilderExtensions
{
    public static SyncMigratorCollectionBuilder Migrators(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<SyncMigratorCollectionBuilder>();

    public static MigrationHandlerCollectionBuilder MigrationHandlers(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<MigrationHandlerCollectionBuilder>();

}
