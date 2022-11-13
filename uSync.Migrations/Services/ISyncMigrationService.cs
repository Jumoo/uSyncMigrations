using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Handlers;
using uSync.Migrations.Models;

namespace uSync.Migrations.Services;
public interface ISyncMigrationService
{
    IEnumerable<ISyncMigrationHandler> GetHandlers();
    IEnumerable<string> HandlerTypes();
    MigrationResults MigrateFiles(MigrationOptions options);
}