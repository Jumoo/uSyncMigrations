using System.Collections;

namespace uSync.Migrations.Models;

public class NewContentTypeInfo
{
    public NewContentTypeInfo(Guid key, string alias, string name, string icon, string? folder)
    {
        Key = key;
        Alias = alias ?? throw new ArgumentNullException(nameof(alias));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Icon = icon ?? throw new ArgumentNullException(nameof(icon));
        Folder = folder;
    }

    public Guid Key { get; set; } = Guid.Empty;

    public string Alias { get; set; }
    public string Name { get; set; }
    public string Icon { get; set; }
    public string? Description { get; set; }
    public bool IsElement { get; set; }

    public string? Folder { get; set; }
    public IList<NewContentTypeProperty> Properties { get; set; } = new List<NewContentTypeProperty>();
    public IList<string> CompositionAliases { get; set; } = new List<string>();
    public IEnumerable<NewContentTypeTab> Tabs { get; set; } = new List<NewContentTypeTab> { new() };
}

public class NewContentTypeTab
{
    public string Name { get; set; } = "Block";
    public string Alias { get; set; } = "block";
    public string Type { get; set; } = "Group";
    public int SortOrder { get; set; } = 0;
}
