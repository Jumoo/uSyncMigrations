using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using uSync.Migrations.Core;

namespace uSync.Migrations.Lite;

/// <summary>
///  Lite Client, is installed with uSync (v13.2), it offers simple 
///  conversion for v8 -> v13 sites. 
/// </summary>
public class MigrationsLiteComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AdduSyncMigrationsLite();
    }
}

public static class MigrationsLiteBuilderExtensions
{
    public static IUmbracoBuilder AdduSyncMigrationsLite(this IUmbracoBuilder builder)
    {
        builder.AdduSyncMigrations();
        return builder;
    }
}


