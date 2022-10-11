using System.Xml.Linq;

using Umbraco.Cms.Core.Notifications;

namespace uSync.Migrations.Notifications;

public class SyncContentTypeMigratedNotification : INotification
{
    public XElement Result { get; set; }

    public SyncContentTypeMigratedNotification(XElement result)
    {
        Result = result;
    }
}
