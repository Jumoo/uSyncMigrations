using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Extensions;

using uSync.Migrations.Composing;
using uSync.Migrations.Configuration;
using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Controllers;

public class uSyncMigrationsController : UmbracoAuthorizedApiController
{
    private readonly SyncMigrationService _migrationService;
    private readonly SyncMigrationConfigurationService _profileConfigService;

    public uSyncMigrationsController(
        SyncMigrationService migrationService,
        SyncMigrationConfigurationService profileConfigService)
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
