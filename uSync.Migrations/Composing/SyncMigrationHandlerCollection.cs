using Umbraco.Cms.Core.Composing;
using uSync.Migrations.Handlers;

namespace uSync.Migrations.Composing;

public class SyncMigrationHandlerCollectionBuilder
    : LazyCollectionBuilderBase<SyncMigrationHandlerCollectionBuilder, SyncMigrationHandlerCollection, ISyncMigrationHandler>
{
    protected override SyncMigrationHandlerCollectionBuilder This => this;
}

public class SyncMigrationHandlerCollection : BuilderCollectionBase<ISyncMigrationHandler>
{
    public SyncMigrationHandlerCollection(Func<IEnumerable<ISyncMigrationHandler>> items)
        : base(items)
    { }

    public IEnumerable<ISyncMigrationHandler> Handlers => this;
}
