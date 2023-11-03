namespace uSync.Migrations.Core.Migrators.Models;

public class ReplacementDataTypeInfo
{
    public ReplacementDataTypeInfo(Guid key, string editorAlias)
    {
        Key = key;
        EditorAlias = editorAlias;
    }

    public Guid Key { get; set; }
    public string EditorAlias { get; set; }

    /// <summary>
    ///  is the replacement to a varied value - if so varies by what?
    /// </summary>
    public string Variation { get; set; } = string.Empty;
}
