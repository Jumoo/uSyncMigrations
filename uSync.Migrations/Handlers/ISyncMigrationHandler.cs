using Umbraco.Cms.Core.Composing;

using uSync.Migrations.Models;

namespace uSync.Migrations.Handlers;

public interface ISyncMigrationHandler : IDiscoverable
{
    public string Group { get; }

    public string ItemType { get; }

    public int Priority { get; }

    public void PrepareMigrations(Guid migrationId, string sourceFolder, SyncMigrationContext context);

    public IEnumerable<MigrationMessage> MigrateFromDisk(Guid migrationId, string sourceFolder, SyncMigrationContext context);
}
