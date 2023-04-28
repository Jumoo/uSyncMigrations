using uSync.Migrations.Configuration.Models;

namespace uSync.Migrations.Configuration;

public interface ISyncMigrationConfigurationService
{
    ISyncMigrationPlan? GetPlan(string planName);
    MigrationPlanInfo GetPlans();
	IEnumerable<ISyncMigrationPlan> GetPlans(string groupAlias);
}