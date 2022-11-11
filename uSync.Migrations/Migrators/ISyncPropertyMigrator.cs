using Umbraco.Cms.Core.Composing;

using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

public interface ISyncPropertyMigrator : IDiscoverable
{
    string[] Editors { get; }

    public string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context);

    public string GetDatabaseType(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context);

    public object GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context);

    public string GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context);
}
