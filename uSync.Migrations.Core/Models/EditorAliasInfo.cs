namespace uSync.Migrations.Core.Models;

public class EditorAliasInfo
{
    public EditorAliasInfo(string orginalEditorAlias, string updatedEditorAlias, Guid? dataTypeDefinition = default)
    {
        OriginalEditorAlias = orginalEditorAlias;
        UpdatedEditorAlias = updatedEditorAlias;
        DataTypeDefinition = dataTypeDefinition;
    }

    public string OriginalEditorAlias { get; }

    public string UpdatedEditorAlias { get; set; }

    public Guid? DataTypeDefinition { get; set; }
}
