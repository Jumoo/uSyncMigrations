using uSync.Migrations.Core.Handlers;
using uSync.Migrations.Core.Models;
using uSync.Migrations.Core.Plans.Models;

namespace uSync.Migrations.Core.Services;

public interface ISyncMigrationService
{
    int DetectVersion(string folder);
    IEnumerable<ISyncMigrationHandler> GetHandlers(int version);
    IEnumerable<string> HandlerTypes(int version);

    MigrationResults MigrateFiles(MigrationOptions options);
    MigrationResults Validate(MigrationOptions? options);
}