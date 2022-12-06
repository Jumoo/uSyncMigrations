using Umbraco.Cms.Core;

using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Handlers;
using uSync.Migrations.Models;

namespace uSync.Migrations.Services;

public interface ISyncMigrationService
{
    IEnumerable<ISyncMigrationHandler> GetHandlers(int version);
    IEnumerable<string> HandlerTypes(int version);

    MigrationResults MigrateFiles(MigrationOptions options);
    MigrationResults Validate(MigrationOptions options);
    Attempt<string> ValidateMigrationSource(string source);
}