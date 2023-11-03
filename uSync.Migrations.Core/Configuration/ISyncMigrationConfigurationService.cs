using uSync.Migrations.Core.Configuration.Models;

namespace uSync.Migrations.Core.Configuration;

public interface ISyncMigrationConfigurationService
{
    ISyncMigrationPlan? GetPlan(string planName);
    MigrationPlanInfo GetPlans();
    IEnumerable<ISyncMigrationPlan> GetPlans(string groupAlias);
}