using Umbraco.Cms.Core.Composing;

using uSync.Migrations.Migrators;

namespace uSync.Migrations.Composing;

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
