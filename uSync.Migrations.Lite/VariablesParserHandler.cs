using Microsoft.AspNetCore.Routing;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

using uSync.Migrations.Lite.Controllers;
using uSync.Migrations.Lite.Services;

namespace uSync.Migrations.Lite;

internal class VariablesParserHandler : INotificationHandler<ServerVariablesParsingNotification>
{
    private readonly LinkGenerator _linkGenerator;

    public VariablesParserHandler(LinkGenerator linkGenerator)
    {
        _linkGenerator = linkGenerator;
    }

    public void Handle(ServerVariablesParsingNotification notification)
    {
        notification.ServerVariables.Add(nameof(uSyncMigrationsLite), new Dictionary<string, object>
        {
            { "conversionService", _linkGenerator.GetUmbracoApiServiceBaseUrl<SyncMigrationSimpleController>(x => x.GetApi())}
        });
    }
   
}