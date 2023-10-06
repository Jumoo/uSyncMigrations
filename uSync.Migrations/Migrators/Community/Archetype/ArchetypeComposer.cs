using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

using uSync.Migrations.Composing;

namespace uSync.Migrations.Migrators.Community.Archetype;

[ComposeAfter(typeof(SyncMigrationsComposer))]
public class ArchetypeComposer : IComposer

{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.TryAddSingleton<IArchetypeMigrationConfigurer, DefaultArchetypeMigrationConfigurer>();
        builder.Services.AddOptions<ArchetypeMigrationOptions>().Configure<IConfiguration>((settings, configuration)
            => configuration.GetSection(ArchetypeMigrationOptions.Section).Bind(settings));
    }
}
