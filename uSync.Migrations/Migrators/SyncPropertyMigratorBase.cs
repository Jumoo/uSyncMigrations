using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

public abstract class SyncPropertyMigratorBase : ISyncPropertyMigrator
{
    public abstract string[] Editors { get; }

    public virtual string GetDatabaseType(string editorAlias, string databaseType, SyncMigrationContext context)
        => databaseType;

    public virtual string GetEditorAlias(string editorAlias, string databaseType, SyncMigrationContext context)
        => editorAlias;

    public virtual object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues, SyncMigrationContext context)
        => preValues.ConvertPreValuesToJson(false);

    public virtual string GetContentValue(string editorAlias, string value, SyncMigrationContext context)
        => value;
}
