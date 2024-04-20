using System.Xml.Linq;

using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Entities;

using uSync.Core;
using uSync.Migrations.Core.Context;
using uSync.Migrations.Core.Services;

namespace uSync.Migrations.Core.Handlers.Shared;

/// <summary>
///  shared base for v7 and v8 migrators
/// </summary>
/// <remarks>
///  the default behaviour of shared migrators is that of v8 (because that is 
///  consistant - so less repeated code)
///  
///  v7 migrators override the default behavors, to do the migrations.
/// </remarks>
public abstract class SharedHandlerBase<TObject> : MigrationHandlerBase<TObject>
    where TObject : IEntity
{
    protected SharedHandlerBase(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        ILogger<SharedHandlerBase<TObject>> _logger)
        : base(eventAggregator, migrationFileService, _logger)
    { }

    /// <summary>
    ///  default for shared is v8 behavoir. 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="context"></param>
    protected override void PrepareFile(XElement source, SyncMigrationContext context)
    { }

    /// <summary>
    ///  alias and key - v8 we have methods that get these values consistently. 
    /// </summary>
    protected override (string alias, Guid key) GetAliasAndKey(XElement source, SyncMigrationContext? context)
        => (alias: source.GetAlias(), key: source.GetKey());

    /// <summary>
    ///  the default in v8 is just to pass the file from source to target.
    /// </summary>
    /// <remarks>
    ///  for the 'basics' like languages etc, this is a copy, when we migrate 
    ///  the complex things (e.g content) we override this even for v8
    /// </remarks>
    protected override XElement? MigrateFile(XElement source, int level, SyncMigrationContext context)
        => source;
}
