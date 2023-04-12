using System.Xml.Linq;

using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using uSync.Migrations.Composing;
using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Handlers.Shared;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Eight;
internal class ContentTypeBaseMigrationHandler<TEntity> : SharedContentTypeBaseHandler<TEntity>
    where TEntity : ContentTypeBase
{
    public ContentTypeBaseMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        ILogger<ContentTypeBaseMigrationHandler<TEntity>> logger,
        IDataTypeService dataTypeService,
        Lazy<SyncMigrationHandlerCollection> migrationHandlers) 
        : base(eventAggregator, migrationFileService, logger, dataTypeService, migrationHandlers)
    { }

    protected override void UpdatePropertyXml(XElement source, XElement newProperty, SyncMigrationContext context)
    {
        // for v8 the properties should match what we are expecting ?
    }

    /// <summary>
    ///  for v8 we clone these into the new file.
    /// </summary>
    protected override void UpdateInfoSection(XElement? info, XElement target, Guid key, SyncMigrationContext context)
    {
        if (info == null) return;
        target.Add(info.Clone());
    }


    protected override void UpdateStructure(XElement source, XElement target)
    {
        var sourceStructure = source.Element("Structure");
        if (sourceStructure != null)
            target.Add(sourceStructure.Clone());
    }

    protected override void UpdateTabs(XElement source, XElement target, SyncMigrationContext context)
    {
        var sourceTabs = source.Element("Tabs");
        if (sourceTabs != null)
            target.Add(sourceTabs.Clone());
    }

    protected override void CheckVariations(XElement target)
    {
        // for v8 we are assuming variations are sound (for now!)
    }

}
