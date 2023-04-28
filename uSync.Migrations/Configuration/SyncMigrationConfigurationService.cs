using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Umbraco.Cms.Core.Extensions;
using Umbraco.Extensions;

using uSync.Migrations.Composing;
using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Configuration;

/// <summary>
///  Service for handling reading/craeting config.
/// </summary>
internal class SyncMigrationConfigurationService : ISyncMigrationConfigurationService
{
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<SyncMigrationConfigurationService> _logger;
    private readonly ISyncMigrationService _migrationService;
    private readonly SyncMigrationProfileCollection _syncMigrationPlans;

    public SyncMigrationConfigurationService(
        IHostEnvironment hostEnvironment,
        ILogger<SyncMigrationConfigurationService> logger,
        ISyncMigrationService migrationService,
        SyncMigrationProfileCollection syncMigrationPlans)
    {
        _hostEnvironment = hostEnvironment;
        _logger = logger;
        _migrationService = migrationService;
        _syncMigrationPlans = syncMigrationPlans;
    }

    public MigrationPlanInfo GetPlans()
    {
        var info = GetCoreInfo();

        // if we have more or less than three its custom.
        if (info.Plans.Count != 3)
        {
            info.HasCustom = true;
        }

        // Load these from disk (/usync/profiles.json ???)
        var custom = GetLocalProfiles();
        if (custom != null)
        {
            info.HasCustom = true;

            if (custom.Remove != null && custom.Remove.Length > 0)
            {
                info.Plans = info.Plans.Where(x => !custom.Remove.InvariantContains(x.Name))
                    .ToList();
            }

            if (custom.Plans != null && custom.Plans.Count > 0)
            {
                info.Plans.AddRange(custom.Plans);
            }
        }

        info.Plans = info.Plans.OrderBy(x => x.Order).ToList();
        return info;
    }

	public IEnumerable<ISyncMigrationPlan> GetPlans(string groupAlias)
        => GetPlans().Plans.Where(x => x.Options.Group.Equals(groupAlias, StringComparison.OrdinalIgnoreCase));


    public ISyncMigrationPlan? GetPlan(string profileName)
        => GetPlans().Plans.FirstOrDefault(x => x.GetType().Name.Equals(profileName));

	private MigrationPlanInfo GetCoreInfo() => new MigrationPlanInfo
    {
        Plans = _syncMigrationPlans.ToList()
    };

    private MigratrionPlanConfig? GetLocalProfiles()
    {
        try
        {
            var profileFile = _hostEnvironment.MapPathContentRoot("~/uSync/profiles.json");

            if (File.Exists(profileFile))
            {
                var content = File.ReadAllText(profileFile);
                var config = JsonConvert.DeserializeObject<MigratrionPlanConfig>(content);

                if (config != null)
                {
                    foreach (var profile in config.Plans)
                    {
                        if (profile == null || profile.Options == null) continue;

                        var configuredHandlers = profile.Options.Handlers?.Select(x => x.Name);
                        if (configuredHandlers == null) continue;

                        profile.Options.Handlers = _migrationService.GetHandlers(profile.Version)
                            .Select(x => new HandlerOption
                            {
                                Name = x.ItemType,
                                Include = configuredHandlers.InvariantContains(x.ItemType)
                            }).ToList();
                    }

                    return config;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load profiles.json");
        }

        return null;
    }
}
