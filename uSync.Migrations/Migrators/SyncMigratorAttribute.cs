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

/// <summary>
///  When there are multiple migrators for a alias, the default one will win. an error is thrown if 
///  there are multiple default migrators. 
/// </summary>
/// <remarks>
///  default migrators can be replaced when loading a profile - but we needed a way of having 
///  multiple migrators for a type, but only picking one in a 'normal' migration.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class SyncDefaultMigratorAttribute : Attribute { }