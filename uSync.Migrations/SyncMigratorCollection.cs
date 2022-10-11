using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;

namespace uSync.Migrations;
public class SyncMigratorCollectionBuilder
    : LazyCollectionBuilderBase<SyncMigratorCollectionBuilder,
        SyncMigratorCollection, ISyncMigrator>
{
    protected override SyncMigratorCollectionBuilder This => this;
}

public class SyncMigratorCollection :
    BuilderCollectionBase<ISyncMigrator>
{
    public SyncMigratorCollection(
        Func<IEnumerable<ISyncMigrator>> items) : base(items)
    { }

    public ISyncMigrator? GetMigrator(string editorAlias)
        => this.FirstOrDefault(x => x.Editors.InvariantContains(editorAlias));
}
