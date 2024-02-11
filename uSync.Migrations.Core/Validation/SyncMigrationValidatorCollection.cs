using Umbraco.Cms.Core.Composing;

namespace uSync.Migrations.Core.Validation;

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
