using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.BackOffice.Controllers;
using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Controllers;

public class uSyncMigrationsController : UmbracoAuthorizedApiController
{
    private readonly SyncMigrationService _migrationService;

    public uSyncMigrationsController(SyncMigrationService migrationService)
    {
        _migrationService = migrationService;
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
}
