namespace uSync.Migrations.Core.Models;

public class PreValue
{
    public int SortOrder { get; set; }
    public string Alias { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
