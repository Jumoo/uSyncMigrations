using System.Xml.Linq;

using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;

using uSync.Migrations.Models;

namespace uSync.Migrations.Notifications;

public class SyncMigratedNotification<TEntity> : StatefulNotification, INotification
    where TEntity : IEntity
{
    public SyncMigrationContext Context { get; set; }

    public XElement Xml { get; set; }

    public SyncMigratedNotification(XElement xml, SyncMigrationContext context)
    {
        Context = context;
        Xml = xml;
    }
}
