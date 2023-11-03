using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

using uSync.Migrations.Core.Composing;
using uSync.Migrations.Migrators.BlockGrid.BlockMigrators;
using uSync.Migrations.Migrators.BlockGrid.Extensions;
using uSync.Migrations.Migrators.BlockGrid.SettingsMigrators;

namespace uSync.Migrations.Migrators.BlockGrid.Composition;
[ComposeAfter(typeof(SyncMigrationsComposer))]
public class BlockGridMigratorComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<GridConventions>();

        builder
            .WithCollectionBuilder<SyncBlockMigratorCollectionBuilder>()
            .Add(() => builder.TypeLoader.GetTypes<ISyncBlockMigrator>());

        builder
            .WithCollectionBuilder<GridSettingsViewMigratorCollectionBuilder>()
            .Add(() => builder.TypeLoader.GetTypes<IGridSettingsViewMigrator>());
    }
}