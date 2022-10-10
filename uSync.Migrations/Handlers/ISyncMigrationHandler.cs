using uSync.Migrations.Models;

namespace uSync.Migrations.Handlers;
public interface ISyncMigrationHandler
{
    public string ItemType { get; }
    public int Priority { get; }

    public void PrepMigrations(Guid migrationId, string sourceFolder, MigrationContext context);

    public IEnumerable<MigrationMessage> MigrateFromDisk(Guid migrationId, string sourceFolder, MigrationContext context);
}
