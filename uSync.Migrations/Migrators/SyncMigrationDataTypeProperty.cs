using System.Collections.ObjectModel;

using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

public sealed class SyncMigrationDataTypeProperty : SyncMigrationPropertyBase
{
    public SyncMigrationDataTypeProperty(string editorAlias, string databaseType, IList<PreValue> preValues)
        : base(editorAlias)
    {
        DatabaseType = databaseType;
        PreValues = new ReadOnlyCollection<PreValue>(preValues);
    }

    public string DatabaseType { get; private set; }

    public IReadOnlyCollection<PreValue> PreValues { get; private set; }
}