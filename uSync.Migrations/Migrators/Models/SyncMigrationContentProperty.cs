namespace uSync.Migrations.Migrators.Models;

/// <summary>
///  content property data passed to a migrator
/// </summary>
public sealed class SyncMigrationContentProperty : SyncMigrationPropertyBase
{
    public SyncMigrationContentProperty(string editorAlias, string? value)
        : base(editorAlias)
    {
        Value = value;
    }

    public string? Value { get; private set; }

}
