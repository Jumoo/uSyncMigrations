using Umbraco.Cms.Core.Composing;

using uSync.Migrations.Core.Validation;

namespace uSync.Migrations.Core.Composing;

public class SyncMigrationValidatorCollectionBuilder
    : LazyCollectionBuilderBase<SyncMigrationValidatorCollectionBuilder, SyncMigrationValidatorCollection, ISyncMigrationValidator>
{
    protected override SyncMigrationValidatorCollectionBuilder This => this;
}

public class SyncMigrationValidatorCollection : BuilderCollectionBase<ISyncMigrationValidator>
{
    public SyncMigrationValidatorCollection(Func<IEnumerable<ISyncMigrationValidator>> items)
        : base(items)
    { }

    public IEnumerable<ISyncMigrationValidator> Validators => this;
}
