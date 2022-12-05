using uSync.Migrations.Migrators;

namespace uSync.Migrations.Models;

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
