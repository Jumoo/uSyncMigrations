namespace uSync.Migrations.Migrators.Models;

public class SplitPropertyContent
{
    public SplitPropertyContent(string alias, string value)
    {
        Alias = alias;
        Value = value;
    }

    public string Alias { get; }

    public string Value { get; }
}
