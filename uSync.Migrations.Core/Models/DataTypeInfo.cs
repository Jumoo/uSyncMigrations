namespace uSync.Migrations.Core.Models;

public class DataTypeInfo
{
    public DataTypeInfo(string editorAlias, string originalEditorAlias, string dataTypeName)
    {
        EditorAlias = editorAlias;
        OriginalEditorAlias = originalEditorAlias;
        DataTypeName = dataTypeName;
    }

    public string EditorAlias { get; }

    public string OriginalEditorAlias { get; set; }
    public string DataTypeName { get; }
}
