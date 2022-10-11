using System.Xml.Linq;

namespace uSync.Migrations.Notifications;

public class SyncMediaTypeMigratingNotification : SyncCancallableMigrationNotificationBase
{
    /// <summary>
    ///  the source XML (from umbraco 7)
    /// </summary>
    public XElement Source { get; set; }

    public SyncMediaTypeMigratingNotification(XElement source)
    {
        Source = source;
    }
}
