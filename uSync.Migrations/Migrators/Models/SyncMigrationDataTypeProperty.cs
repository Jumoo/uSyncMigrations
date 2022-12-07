using System.Collections.ObjectModel;

using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.Models;

public sealed class SyncMigrationDataTypeProperty : SyncMigrationPropertyBase
{
    public SyncMigrationDataTypeProperty(string editorAlias, string databaseType, IList<PreValue> preValues)
        : base(editorAlias)
    {
        DatabaseType = databaseType;
        PreValues = new ReadOnlyCollection<PreValue>(preValues);
    }

    public SyncMigrationDataTypeProperty(string editorAlias, string databaseType, string? config)
        : base(editorAlias) 
    {
        DatabaseType = databaseType;
        ConfigAsString = config;
    }

    public string DatabaseType { get; private set; }

    public IReadOnlyCollection<PreValue>? PreValues { get; private set; }

    public string? ConfigAsString { get; private set; }
}