using Newtonsoft.Json;

using Umbraco.Cms.Core;
using Umbraco.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations;
public abstract class SyncMigratorBase : ISyncMigrator
{
    public abstract string[] Editors { get; }

    public virtual string GetDatabaseType(string editorAlias, string databaseType)
        => databaseType;

    public virtual string GetEditorAlias(string editorAlias, string dabaseType)
        => editorAlias;

    public virtual object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
    {
        throw new NotImplementedException();
    }

    public virtual string GetContentValue(string editorAlias, string value)
        => value;
}
