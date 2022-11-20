using Umbraco.Cms.Core.Composing;

using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Validation;

namespace uSync.Migrations.Composing;

public class SyncMigrationProfileCollectionBuilder
    : WeightedCollectionBuilderBase<SyncMigrationProfileCollectionBuilder, SyncMigrationProfileCollection, ISyncMigrationProfile>
{
    protected override SyncMigrationProfileCollectionBuilder This => this;
}


public class SyncMigrationProfileCollection : BuilderCollectionBase<ISyncMigrationProfile>
{
    public SyncMigrationProfileCollection(Func<IEnumerable<ISyncMigrationProfile>> items)
        : base(items)
    { }

    public IEnumerable<ISyncMigrationProfile> Profiles => this.OrderBy(x => x.Order);
}

