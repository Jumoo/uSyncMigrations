using Umbraco.Cms.Core.Notifications;

namespace uSync.Migrations.Notifications;
public class SyncCancallableMigrationNotificationBase : ICancelableNotification
{
    public bool Cancel { get; set; }
}
