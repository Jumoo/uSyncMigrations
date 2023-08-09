namespace uSync.Migrations.Models;

public class NewContentTypeProperty
{
    public string Name { get; set; }
    public string Alias { get; set; }
    public string DataTypeAlias { get; set; }
    public string OriginalEditorAlias { get; set; }

    public string TabAlias { get; set; } = "block";
}