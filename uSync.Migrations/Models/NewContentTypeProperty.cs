namespace uSync.Migrations.Models;

public class NewContentTypeProperty
{
    public NewContentTypeProperty(string name, string alias, string dataTypeAlias)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Alias = alias ?? throw new ArgumentNullException(nameof(alias));
        DataTypeAlias = dataTypeAlias ?? throw new ArgumentNullException(nameof(dataTypeAlias));
    }

    public NewContentTypeProperty(string name, string alias, string dataTypeAlias, string orginalEditorAlias)
        : this(name, alias, dataTypeAlias)
    {
        OriginalEditorAlias = orginalEditorAlias;
    }


    public string Name { get; set; }
    public string Alias { get; set; }
    public string DataTypeAlias { get; set; }
    
    public string? OriginalEditorAlias { get; set; }
    public string TabAlias { get; set; } = "block";
}