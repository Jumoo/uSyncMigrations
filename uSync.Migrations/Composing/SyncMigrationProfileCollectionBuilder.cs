using Umbraco.Cms.Core.Composing;

using uSync.Migrations.Configuration.Models;

namespace uSync.Migrations.Composing;

public class SyncMigrationProfileCollectionBuilder
    : LazyCollectionBuilderBase<SyncMigrationProfileCollectionBuilder, SyncMigrationProfileCollection, ISyncMigrationProfile>
{
    protected override SyncMigrationProfileCollectionBuilder This => this;
}
