namespace uSync.Migrations.Migrators.Models;

/// <summary>
///  content property data passed to a migrator
/// </summary>
public sealed class SyncMigrationContentProperty : SyncMigrationPropertyBase
{
    public string ContentTypeAlias { get; set; }
    public string PropertyAlias { get; set; }

    [Obsolete("Pass in ContentTypeAlias and PropertyAlias to enable more granular control")]
    public SyncMigrationContentProperty(
        string editorAlias, string? value)
        : base(editorAlias)
    {
        Value = value;

        // fallback values
        ContentTypeAlias = editorAlias;
        PropertyAlias = editorAlias;
    }

    public SyncMigrationContentProperty(
        string contentTypeAlias,
        string propertyAlias,
        string editorAlias, string? value)
        : this(editorAlias, value)
    {
        ContentTypeAlias = contentTypeAlias;
        PropertyAlias = propertyAlias;  
    }

    public string? Value { get; private set; }

}
