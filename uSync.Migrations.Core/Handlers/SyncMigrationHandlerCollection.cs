using Umbraco.Cms.Core.Composing;

using uSync.Migrations.Core.Extensions;
using uSync.Migrations.Core.Plans.Models;

namespace uSync.Migrations.Core.Handlers;

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

    public IList<HandlerOption> SelectGroup(int version, string group)
        => Handlers
            .Where(x => x.SourceVersion == version)
            .Select(x => x.ToHandlerOption(group == "" || x.Group == group))
            .ToList();

}
