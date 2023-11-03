using Umbraco.Cms.Core.Composing;

using uSync.Migrations.Core.Migrators;

namespace uSync.Migrations.Core.Composing;

public class SyncPropertyMergingCollectionBuilder


    : OrderedCollectionBuilderBase<SyncPropertyMergingCollectionBuilder,
        SyncPropertyMergingCollection, ISyncPropertyMergingMigrator>
{
    protected override SyncPropertyMergingCollectionBuilder This => this;
}

public class SyncPropertyMergingCollection
    : BuilderCollectionBase<ISyncPropertyMergingMigrator>
{
    public SyncPropertyMergingCollection(Func<IEnumerable<ISyncPropertyMergingMigrator>> items)
        : base(items)
    { }

    public ISyncPropertyMergingMigrator? GetByName(string name)
        => this.FirstOrDefault(x => x.GetType().Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}
