using Org.BouncyCastle.Bcpg.Sig;

using uSync.Migrations.Models;

namespace uSync.Migrations;
public interface ISyncItemMigrator
{
    /// <summary>
    ///  editors this migration is good for. 
    /// </summary>
    string[] Editors { get; }
}

/// <summary>
///  handle a editor alias during migration.
/// </summary>

public interface ISyncMigrator
{
    string[] Editors { get; }

    public string GetEditorAlias(string editorAlias, string dabaseType);

    public string GetDatabaseType(string editorAlias, string databaseType);

    public object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues);

    public string GetContentValue(string editorAlias, string value);
}
