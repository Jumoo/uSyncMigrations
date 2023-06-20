using uSync.Migrations.Migrators;

namespace uSync.Migrations.Models;
public class MergingPropertiesConfig
{
    public MergingPropertiesConfig(string targetProperty, string mergingMigrator)
    {
        TargetPropertyAlias = targetProperty;
        Merger = mergingMigrator;
        RemoveMergedProperties = true;
    }

    public MergingPropertiesConfig(string targetProperty, string mergingMigrator, IEnumerable<string> properties)
        : this(targetProperty, mergingMigrator)
    {
        MergedProperties.AddRange(properties);
    }

    public List<string> MergedProperties { get; set; } = new();
    public string TargetPropertyAlias { get; set; }
    public string Merger { get; set; }
    public bool RemoveMergedProperties { get; set; }
}

public class MergingPropertyValue
{
    public MergingPropertyValue(string propertyAlias, string editorAlias, string value)
    {
        PropertyAlias = propertyAlias;
        EditorAlias = editorAlias;
        Value = value;
    }

    public string PropertyAlias { get; set; }
    public string EditorAlias { get; set; }
    public string Value { get; set; }   
}