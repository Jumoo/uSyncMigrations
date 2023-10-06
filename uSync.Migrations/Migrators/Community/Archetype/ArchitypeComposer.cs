using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

using uSync.Migrations.Composing;

namespace uSync.Migrations.Migrators.Community.Archetype;

[ComposeAfter(typeof(SyncMigrationsComposer))]
public class ArchitypeComposer : IComposer

{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<ArchetypeMigrationConfigurerCollectionBuilder>()
            .Add(() => builder.TypeLoader.GetTypes<IArchetypeMigrationConfigurer>());

        builder.Services.AddOptions<ArchetypeMigrationOptions>().Configure<IConfiguration>((settings, configuration)
            => configuration.GetSection(ArchetypeMigrationOptions.Section).Bind(settings));
    }
}
