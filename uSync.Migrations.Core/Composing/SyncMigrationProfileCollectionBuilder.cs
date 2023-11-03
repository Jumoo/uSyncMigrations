using Umbraco.Cms.Core.Composing;

using uSync.Migrations.Core.Configuration.Models;

namespace uSync.Migrations.Core.Composing;

public class SyncMigrationProfileCollectionBuilder
    : WeightedCollectionBuilderBase<SyncMigrationProfileCollectionBuilder, SyncMigrationProfileCollection, ISyncMigrationPlan>
{
    protected override SyncMigrationProfileCollectionBuilder This => this;
}

public class SyncMigrationProfileCollection : BuilderCollectionBase<ISyncMigrationPlan>
{
    public SyncMigrationProfileCollection(Func<IEnumerable<ISyncMigrationPlan>> items)
        : base(items)
    { }

    public IEnumerable<ISyncMigrationPlan> Profiles => this.OrderBy(x => x.Order);
}
