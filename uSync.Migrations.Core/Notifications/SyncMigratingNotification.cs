using System.Xml.Linq;

using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;

using uSync.Migrations.Core.Context;

namespace uSync.Migrations.Core.Notifications;

public class SyncMigratingNotification<TEntity> : StatefulNotification, ICancelableNotification
    where TEntity : IEntity
{
    public bool Cancel { get; set; }

    public SyncMigrationContext Context { get; set; }

    public XElement LegacyXml { get; set; }

    public SyncMigratingNotification(XElement xml, SyncMigrationContext context)
    {
        Context = context;
        LegacyXml = xml;
    }
}