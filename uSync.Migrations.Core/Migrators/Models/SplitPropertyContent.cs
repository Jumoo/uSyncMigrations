namespace uSync.Migrations.Core.Migrators.Models;

public class SplitPropertyContent
{
    public SplitPropertyContent(string alias, object value)
    {
        Alias = alias;
        Value = value;
    }

    public string Alias { get; }

    public object Value { get; }
}
