using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Umbraco.Cms.Core.Composing;

namespace uSync.Migrations.Handlers;
/// <summary>
/// Collection builder for SyncHandlers
/// </summary>
public class MigrationHandlerCollectionBuilder
    : LazyCollectionBuilderBase<MigrationHandlerCollectionBuilder, 
        MigrationHandlerCollection,
        ISyncMigrationHandler>
{
    /// <inheritdoc/>
    protected override MigrationHandlerCollectionBuilder This => this;
}

/// <summary>
/// A collection of SyncHandlers
/// </summary>
public class MigrationHandlerCollection : BuilderCollectionBase<ISyncMigrationHandler>
{
    /// <summary>
    ///  Construct a collection of handlers from a list of handler items 
    /// </summary>
    public MigrationHandlerCollection(Func<IEnumerable<ISyncMigrationHandler>> items)
        : base(items)
    { }

    /// <summary>
    ///  Handlers in the collection
    /// </summary>
    public IEnumerable<ISyncMigrationHandler> Handlers => this;


}
