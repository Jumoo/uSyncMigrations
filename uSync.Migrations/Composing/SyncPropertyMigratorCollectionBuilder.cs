using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;

using uSync.Migrations.Migrators;
using uSync.Migrations.Models;

namespace uSync.Migrations.Composing;

public class SyncPropertyMigratorCollectionBuilder
    : OrderedCollectionBuilderBase<SyncPropertyMigratorCollectionBuilder, SyncPropertyMigratorCollection, ISyncPropertyMigrator>
{
    protected override SyncPropertyMigratorCollectionBuilder This => this;
}

public class SyncPropertyMigratorCollection
    : BuilderCollectionBase<ISyncPropertyMigrator>
{
    public SyncPropertyMigratorCollection(Func<IEnumerable<ISyncPropertyMigrator>> items)
        : base(items)
    { }

    public IList<MigratorEditorPair> GetPreferedMigratorList(IDictionary<string, string> preferedMigrators)
    {
        var migrators = this.ToList();
        var defaultMigrators = GetDefaults();

        var editors = new List<MigratorEditorPair>();
        foreach (var migrator in migrators)
        {
            foreach (var editor in migrator.Editors)
            {
                if (preferedMigrators != null && preferedMigrators.ContainsKey(editor))
                {
                    var syncMigrator = migrators.FirstOrDefault(x => x.GetType().Name == preferedMigrators[editor]) ?? migrator;
                    editors.Add(new MigratorEditorPair(editor, syncMigrator));
                }
                else
                {
                    if (defaultMigrators.ContainsKey(editor))
                    {
                        // there is a default for this one, so its the only one we add.
                        editors.Add(new MigratorEditorPair(editor, defaultMigrators[editor]));
                    }
                    else
                    {
                        // we just add what ever this is 
                        editors.Add(new MigratorEditorPair(editor, migrator));
                    }
                }
            }
        }

        // remove duplicates (we might add prefered or default multiple times).
        return editors.DistinctBy(x => $"{x.EditorAlias}_{x.Migrator.GetType().Name}").ToList();
    }

    private IDictionary<string, ISyncPropertyMigrator> GetDefaults()
    {
        var defaults = new Dictionary<string, ISyncPropertyMigrator>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in this.Where(x => x.GetType().GetCustomAttribute<SyncDefaultMigratorAttribute>(false) != null))
        {
            foreach (var editor in item.Editors)
            {
                defaults[editor] = item;
            }
        }
        return defaults;
    }

}