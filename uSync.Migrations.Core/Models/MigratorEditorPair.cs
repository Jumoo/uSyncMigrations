using uSync.Migrations.Core.Migrators;

namespace uSync.Migrations.Core.Models;

public class MigratorEditorPair
{
    public MigratorEditorPair(string editorAlias, ISyncPropertyMigrator migrator)
    {
        EditorAlias = editorAlias;
        Migrator = migrator;
    }

    public string EditorAlias { get; set; }
    public ISyncPropertyMigrator Migrator { get; set; }

}
