using System.Xml.Linq;

using Umbraco.Cms.Core.Notifications;

namespace uSync.Migrations.Notifications;

public class SyncMediaTypeMigratedNotification : INotification
{
    public XElement Result { get; set; }

    public SyncMediaTypeMigratedNotification(XElement result)
    {
        Result = result;
    }
}