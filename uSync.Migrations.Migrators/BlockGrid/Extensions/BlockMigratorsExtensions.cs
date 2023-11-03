using Umbraco.Cms.Core.DependencyInjection;

using uSync.Migrations.Migrators.BlockGrid.BlockMigrators;

namespace uSync.Migrations.Migrators.BlockGrid.Extensions;
public static class BlockMigratorsExtensions
{
    public static SyncBlockMigratorCollectionBuilder SyncBlockMigrators(this IUmbracoBuilder builder)
    => builder.WithCollectionBuilder<SyncBlockMigratorCollectionBuilder>();

}
