using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

using uSync.Migrations.Core.Composing;

namespace uSync.Migrations.Client;

public class uSyncMigrationsComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AdduSyncMigrations();
        builder.AdduSyncMigrationsUI();
    }
}

internal static class uSyncMigrationsBuilderExtensions
{
    public static IUmbracoBuilder AdduSyncMigrationsUI(this IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ServerVariablesParsingNotification, SyncMigrationsServerVariablesParsingNotificationHandler>();
        return builder;
    }
}
