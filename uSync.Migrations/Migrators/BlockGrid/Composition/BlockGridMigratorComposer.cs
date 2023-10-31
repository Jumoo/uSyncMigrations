using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using uSync.Migrations.Composing;
using uSync.Migrations.Migrators.BlockGrid.SettingsMigrator;

namespace uSync.Migrations.Migrators.BlockGrid.Composition;
[ComposeAfter(typeof(SyncMigrationsComposer))]
public class BlockGridMigratorComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder
            .WithCollectionBuilder<GridSettingsViewMigratorCollectionBuilder>()
            .Add(() => builder.TypeLoader.GetTypes<IGridSettingsViewMigrator>());
    }
}