using Microsoft.AspNetCore.Mvc;

using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Extensions;

using uSync.Migrations.Configuration;
using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Controllers;

public class uSyncMigrationsController : UmbracoAuthorizedApiController
{
    private readonly ISyncMigrationService _migrationService;
    private readonly ISyncMigrationConfigurationService _profileConfigService;

    public uSyncMigrationsController(
        ISyncMigrationService migrationService,
        ISyncMigrationConfigurationService profileConfigService)
    {
        _migrationService = migrationService;
        _profileConfigService = profileConfigService;
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

    [HttpGet]
    public MigrationProfileInfo GetProfiles()
        => _profileConfigService.GetProfiles();
}
