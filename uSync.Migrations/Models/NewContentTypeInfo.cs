namespace uSync.Migrations.Models;

public class NewContentTypeInfo
{
    public Guid Key { get; set; } = Guid.Empty;

    public string Alias { get; set; }
    public string Name { get; set; }
    public string Icon { get; set; }
    public string? Description { get; set; }
    public bool IsElement { get; set; }

    public string? Folder { get; set; }
    public IList<NewContentTypeProperty> Properties { get; set; } = new List<NewContentTypeProperty>();
}
