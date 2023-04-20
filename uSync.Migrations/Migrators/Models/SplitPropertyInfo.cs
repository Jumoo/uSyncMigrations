namespace uSync.Migrations.Migrators.Models;

public class SplitPropertyInfo
{
    public SplitPropertyInfo(string name, string alias, string dataTypeAlias, Guid dataTypeDefinition)
    {
        Name = name;
        Alias = alias;
        DataTypeAlias = dataTypeAlias;
        DataTypeDefinition = dataTypeDefinition;
    }

    public string Name { get; }
    public string Alias { get; }
    public string DataTypeAlias { get; }
    public Guid DataTypeDefinition { get; }
}
