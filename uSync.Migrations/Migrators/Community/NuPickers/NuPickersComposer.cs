using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

using uSync.Migrations.Composing;

namespace uSync.Migrations.Migrators.Community.NuPickers;

[ComposeAfter(typeof(SyncMigrationsComposer))]
public class NuPickersComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddOptions<NuPickerMigrationOptions>().Configure<IConfiguration>((settings, configuration)
            => configuration.GetSection(NuPickerMigrationOptions.Section).Bind(settings));

    }
}
