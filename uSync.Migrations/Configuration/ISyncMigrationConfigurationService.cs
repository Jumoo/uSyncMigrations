using uSync.Migrations.Configuration.Models;

namespace uSync.Migrations.Configuration;

public interface ISyncMigrationConfigurationService
{
    MigrationProfileInfo GetProfiles();
}