using Umbraco.Cms.Core.Composing;
using uSync.Migrations.Migrators;

namespace uSync.Migrations.Composing;

public class SyncPropertyMigratorCollectionBuilder
    : OrderedCollectionBuilderBase<SyncPropertyMigratorCollectionBuilder, SyncPropertyMigratorCollection, ISyncPropertyMigrator>
{
    protected override SyncPropertyMigratorCollectionBuilder This => this;
}

public class SyncPropertyMigratorCollection
    : BuilderCollectionBase<ISyncPropertyMigrator>
{
    private readonly Dictionary<string, ISyncPropertyMigrator> _lookup;

    public SyncPropertyMigratorCollection(Func<IEnumerable<ISyncPropertyMigrator>> items)
        : base(items)
    {
        _lookup = new Dictionary<string, ISyncPropertyMigrator>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in this)
        {
            foreach (var alias in item.Editors)
            {
                _ = _lookup.TryAdd(alias, item);
            }
        }
    }

    public bool TryGet(string editorAlias, out ISyncPropertyMigrator? item) => _lookup.TryGetValue(editorAlias, out item);

    public ISyncPropertyMigrator? Get(string editorAlias) => _lookup.TryGetValue(editorAlias, out var migrator) == true ? migrator : default;
}
