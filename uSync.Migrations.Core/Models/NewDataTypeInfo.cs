namespace uSync.Migrations.Core.Models;

public class NewDataTypeInfo
{
    public NewDataTypeInfo(Guid key, string alias, string name, string editorAlias, string databaseType, object? config)
    {
        Key = key;
        Alias = alias ?? throw new ArgumentNullException(nameof(alias));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        EditorAlias = editorAlias ?? throw new ArgumentNullException(nameof(editorAlias));
        DatabaseType = databaseType ?? throw new ArgumentNullException(nameof(databaseType));
        Config = config;
    }

    public Guid Key { get; set; } = Guid.Empty;

    public string Alias { get; set; }

    public string Name { get; set; }

    public string EditorAlias { get; set; }

    public string DatabaseType { get; set; }

    public object? Config { get; set; }
}