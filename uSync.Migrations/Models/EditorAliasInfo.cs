namespace uSync.Migrations.Models;

public class EditorAliasInfo
{
    public EditorAliasInfo(string orginalEditorAlias, string updatedEditorAlias, Guid? dataTypeDefinition = default)
    {
        OriginalEditorAlias = orginalEditorAlias;
        UpdatedEditorAlias = updatedEditorAlias;
        DataTypeDefinition = dataTypeDefinition;
    }

    public string OriginalEditorAlias { get; }

	public string UpdatedEditorAlias { get; }

    public Guid? DataTypeDefinition { get; set; }
}
