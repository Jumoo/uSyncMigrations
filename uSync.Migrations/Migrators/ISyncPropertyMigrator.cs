using Umbraco.Cms.Core.Composing;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

public interface ISyncPropertyMigrator : IDiscoverable
{
    string[] Editors { get; }

    public string GetEditorAlias(string editorAlias, string databaseType, SyncMigrationContext context);

    public string GetDatabaseType(string editorAlias, string databaseType, SyncMigrationContext context);

    public object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues, SyncMigrationContext context);

    public string GetContentValue(string editorAlias, string value, SyncMigrationContext context);
}
