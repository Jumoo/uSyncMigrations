using Microsoft.AspNetCore.Routing;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

using uSync.Migrations.Client.Controllers;
using uSync.Migrations.Core;

namespace uSync.Migrations.Client;

internal class SyncMigrationsServerVariablesParsingNotificationHandler : INotificationHandler<ServerVariablesParsingNotification>
{
    private readonly LinkGenerator _linkGenerator;

    public SyncMigrationsServerVariablesParsingNotificationHandler(LinkGenerator linkGenerator)
    {
        _linkGenerator = linkGenerator;
    }

    public void Handle(ServerVariablesParsingNotification notification)
    {
        notification.ServerVariables.Add(nameof(uSyncMigrations), new Dictionary<string, object>
        {
            { "migrationService", _linkGenerator.GetUmbracoApiServiceBaseUrl<uSyncMigrationsController>(x => x.GetApi()) ?? "/umbraco/backoffice/api/usyncmigrations/" }
        });
    }
}
