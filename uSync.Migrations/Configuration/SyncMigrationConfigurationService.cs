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
    private readonly SyncMigrationProfileCollection _syncMigrationProfiles;

    public SyncMigrationConfigurationService(
        IHostEnvironment hostEnvironment,
        ILogger<SyncMigrationConfigurationService> logger,
        ISyncMigrationService migrationService,
        SyncMigrationProfileCollection syncMigrationProfiles)
    {
        _hostEnvironment = hostEnvironment;
        _logger = logger;
        _migrationService = migrationService;
        _syncMigrationProfiles = syncMigrationProfiles;
    }

    public MigrationProfileInfo GetProfiles()
    {
        var info = GetCoreInfo();

        // if we have more or less than three its custom.
        if (info.Profiles.Count != 3)
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
                info.Profiles = info.Profiles.Where(x => !custom.Remove.InvariantContains(x.Name))
                    .ToList();
            }

            if (custom.Profiles != null && custom.Profiles.Count > 0)
            {
                info.Profiles.AddRange(custom.Profiles);
            }
        }

        info.Profiles = info.Profiles.OrderBy(x => x.Order).ToList();
        return info;
    }

    private MigrationProfileInfo GetCoreInfo() => new MigrationProfileInfo
    {
        Profiles = _syncMigrationProfiles.ToList()
    };

    private MigratrionProfileConfig? GetLocalProfiles()
    {
        try
        {
            var profileFile = _hostEnvironment.MapPathContentRoot("~/uSync/profiles.json");

            if (File.Exists(profileFile))
            {
                var content = File.ReadAllText(profileFile);
                var config = JsonConvert.DeserializeObject<MigratrionProfileConfig>(content);

                if (config != null)
                {
                    foreach (var profile in config.Profiles)
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
