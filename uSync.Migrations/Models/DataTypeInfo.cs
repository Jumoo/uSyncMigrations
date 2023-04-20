namespace uSync.Migrations.Models;

public class DataTypeInfo
{
    public DataTypeInfo(string editorAlias, string dataTypeName)
    {
        EditorAlias = editorAlias;
        DataTypeName = dataTypeName;
    }

    public string EditorAlias { get; }

	public string DataTypeName { get; }
}
