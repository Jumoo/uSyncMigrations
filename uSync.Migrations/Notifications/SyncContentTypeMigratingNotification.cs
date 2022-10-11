using System.Xml.Linq;

namespace uSync.Migrations.Notifications;
public class SyncContentTypeMigratingNotification : SyncCancallableMigrationNotificationBase
{
    /// <summary>
    ///  the source XML (from umbraco 7)
    /// </summary>
    public XElement Source { get; set; }

    public SyncContentTypeMigratingNotification(XElement source)
    {
        Source = source;
    }
}
