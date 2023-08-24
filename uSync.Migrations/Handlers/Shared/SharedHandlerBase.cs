using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Entities;

using uSync.Core;
using uSync.Migrations.Configuration;
using uSync.Migrations.Context;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Shared;

/// <summary>
///  shared base for v7 and v8 migrators
/// </summary>
/// <remarks>
///  the default behaviour of shared migrators is that of v8 (because that is 
///  consistant - so less repeated code)
///  
///  v7 migrators override the default behavors, to do the migrations.
/// </remarks>
internal abstract class SharedHandlerBase<TObject> :MigrationHandlerBase<TObject>
    where TObject : IEntity
{
    private readonly IOptions<uSyncMigrationOptions> _options;

    protected SharedHandlerBase(
        IOptions<uSyncMigrationOptions> options,
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        ILogger<SharedHandlerBase<TObject>> _logger) 
        : base(eventAggregator, migrationFileService, _logger)
    {
        _options = options;
    }

    /// <summary>
    ///  default for shared is v8 behavoir. 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="context"></param>
    protected override void PrepareFile(XElement source, SyncMigrationContext context)
    { }

    /// <summary>
    ///  alias and key - v8 we have methods that get these values consitantly. 
    /// </summary>
    protected override (string alias, Guid key) GetAliasAndKey(XElement source)
        => (alias: PrepareAlias(source.GetAlias()), key: source.GetKey());

    private string PrepareAlias(string alias)
    {
        if (_options.Value?.OverrideAliases?.ContainsKey(typeof(TObject).Name) == true && _options.Value?.OverrideAliases[typeof(TObject).Name]?.ContainsKey(alias) == true)
        {
            return _options.Value?.OverrideAliases[typeof(TObject).Name][alias];
        }

        return alias;
    }
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
