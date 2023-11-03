namespace uSync.Migrations.Core.Migrators;

/// <summary>
///  base model for all property data passed to migratiors
/// </summary>
public class SyncMigrationPropertyBase
{
    public SyncMigrationPropertyBase(string editorAlias)
    {
        EditorAlias = editorAlias;
    }

    public string EditorAlias { get; protected set; }
}
