using uSync.Migrations.Core.Plans;
using uSync.Migrations.Core.Plans.Models;

namespace uSync.Migrations.Core.Configuration;

public interface ISyncMigrationConfigurationService
{
    ISyncMigrationPlan? GetPlan(string planName);
    MigrationPlanInfo GetPlans();
    IEnumerable<ISyncMigrationPlan> GetPlans(string groupAlias);
}