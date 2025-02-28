using System.Xml.Linq;

using Microsoft.Extensions.Logging;

using NUglify.Helpers;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

using uSync.Migrations.Core.Context;
using uSync.Migrations.Core.Extensions;
using uSync.Migrations.Core.Handlers.Shared;
using uSync.Migrations.Core.Services;

namespace uSync.Migrations.Core.Handlers.Eight;
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

    protected override (string alias, Guid key) GetAliasAndKey(XElement source, SyncMigrationContext? context)
    {
        var (alias, key) = base.GetAliasAndKey(source, context);
        return (alias: context?.ContentTypes.GetReplacementAlias(alias) ?? alias, key);
    }

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
        var targetInfo = info.Clone();
        if (targetInfo != null)
        {
            targetInfo.Element("Compositions")?.Elements("Composition").ForEach(c => c.Value = context.ContentTypes.GetReplacementAlias(c.Value));

            target.Add(targetInfo);
        }
    }


    protected override void UpdateStructure(XElement source, XElement target, SyncMigrationContext context)
    {
        var sourceStructure = source.Element("Structure");

        if (sourceStructure != null)
        {
            var targetStructure = sourceStructure.Clone();
            targetStructure!
                .Elements()
                .Select(e => e.Element("ContentType"))
                .WhereNotNull()
                .ForEach(e => e.Value = context.ContentTypes.GetReplacementAlias(e.Value));
            target.Add(targetStructure);
        }
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
