using Umbraco.Cms.Core.Models;

namespace uSync.Migrations.Core.Models;

public class DataTypeInfo
{
	public DataTypeInfo(IDataType dataType)
    	: this(dataType.EditorAlias, dataType.EditorAlias, dataType.Name ?? dataType.EditorAlias)
	{ }

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
