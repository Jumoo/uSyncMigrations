using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Extensions;

using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Controllers;

public class uSyncMigrationsController : UmbracoAuthorizedApiController
{
    private readonly SyncMigrationService _migrationService;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<uSyncMigrationsController> _logger;

    public uSyncMigrationsController(
        SyncMigrationService migrationService,
        IHostEnvironment hostingEnvironment,
        ILogger<uSyncMigrationsController> logger)
    {
        _migrationService = migrationService;
        _hostEnvironment = hostingEnvironment;
        _logger = logger;
    }

    [HttpGet]
    public bool GetApi() => true;

    /// <summary>
    ///  looks to see if there is a usync/data folder, and if there is
    ///  if we have migrated it in the past.
    /// </summary>
    [HttpGet]
    public bool HasPendingMigration() => true;

    [HttpGet]
    public object GetMigrationOptions()
    {
        return new
        {
            hasPending = true,
            handlers = _migrationService.HandlerTypes().Select(x => new HandlerOption { Name = x, Include = false })
        };
    }

    [HttpPost]
    public MigrationResults Migrate(MigrationOptions options)
    {
        return _migrationService.MigrateFiles(options);
    }

    public static string _dateMask = "yyyyMMdd_HHmm";

    [HttpGet]
    public MigrationProfileInfo GetProfiles()
    {
        var info = GetCoreInfo();

        // Load these from disk (/usync/profiles.json ???)
        var custom = GetLocalProfiles();
        if (custom != null)
        {
            info.HasCustom = true;

            if (custom.Remove != null && custom.Remove.Length > 0)
            {
                info.Profiles = info.Profiles.Where(x => !custom.Remove.InvariantContains(x.Name)).ToList();
            }

            if (custom.Profiles != null && custom.Profiles.Count > 0)
            {
                info.Profiles.AddRange(custom.Profiles);
            }
        }

        return info;
    }

    private MigrationProfileInfo GetCoreInfo()
    {
        return new MigrationProfileInfo
        {
            Profiles = new List<MigrationProfile>
            {
                new MigrationProfile
                {
                    Name = "Settings",
                    Icon = "icon-settings-alt color-blue",
                    Description = "Migrate all the settings",
                    Options = new MigrationOptions
                    {
                        Target = $"{uSyncMigrations.MigrationFolder}/{DateTime.Now.ToString(_dateMask)}",
                        Handlers = _migrationService.GetHandlers()
                            .Select(x => new HandlerOption
                            {
                                Name = x.ItemType,
                                Include = x.Group == BackOffice.uSyncConstants.Groups.Settings
                            })
                    }
                },
                new MigrationProfile {
                    Name = "Content",
                    Icon = "icon-documents color-purple",
                    Description = "Migrate all the content",
                    Options = new MigrationOptions
                    {
                        Target = $"{uSyncMigrations.MigrationFolder}/{DateTime.Now.ToString(_dateMask)}",
                        Handlers = _migrationService.GetHandlers()
                            .Select(x => new HandlerOption
                            {
                                Name = x.ItemType,
                                Include = x.Group == BackOffice.uSyncConstants.Groups.Content
                            })
                    }
                },
                new MigrationProfile
                {
                    Name = "Everything",
                    Description = "Migrate everything",
                    Icon = "icon-paper-plane color-orange",
                    Options = new MigrationOptions
                    {
                        Target = $"{uSyncMigrations.MigrationFolder}/{DateTime.Now.ToString(_dateMask)}",
                        Handlers = _migrationService.GetHandlers()
                            .Select(x => new HandlerOption
                            {
                                Name = x.ItemType,
                                Include = true
                            })
                    }
                }
            }
        };
    }


    private MigratrionProfileConfig? GetLocalProfiles()
    {
        try
        {
            var profileFile = _hostEnvironment.MapPathContentRoot("~/uSync/profiles.json");

            if (System.IO.File.Exists(profileFile))
            {
                var content = System.IO.File.ReadAllText(profileFile);
                var config = JsonConvert.DeserializeObject<MigratrionProfileConfig>(content);

                if (config != null) 
                {
                    foreach (var profile in config.Profiles)
                    {
                        var configuredHandlers = profile.Options.Handlers.Select(x => x.Name);
                        profile.Options.Handlers = _migrationService.GetHandlers()
                            .Select(x => new HandlerOption
                            {
                                Name = x.ItemType,
                                Include = configuredHandlers.InvariantContains(x.ItemType)
                            });
                    }

                    return config;
                }
            }
        }
        catch(Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load profiles.json");
        }

        return null;


    }
}
