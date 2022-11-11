namespace uSync.Migrations.Migrators;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SyncMigratorAttribute : Attribute
{
    /// <summary>
    ///  Use this attributes EditorAlias as the default (and return it when asked)
    /// </summary>
    public bool IsDefaultAlias { get; set; }

    public string EditorAlias { get; set; }

    public Type? ConfigurationType { get; set; }

    public SyncMigratorAttribute(string editorAlias)
    {
        EditorAlias = editorAlias;
    }

    public SyncMigratorAttribute(string editorAlias, Type configurationType) : this(editorAlias)
    {
        ConfigurationType = configurationType;
    }
}
