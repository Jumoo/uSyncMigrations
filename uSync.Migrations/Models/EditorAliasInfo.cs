namespace uSync.Migrations.Models;

public class EditorAliasInfo
{
	public EditorAliasInfo(string orginalEditorAlias, string updatedEditorAlias)
	{
		OriginalEditorAlias = orginalEditorAlias;
		UpdatedEditorAlias = updatedEditorAlias;
	}

	public string OriginalEditorAlias { get; }

	public string UpdatedEditorAlias { get; }
}
