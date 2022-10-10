using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

using Umbraco.Cms.Web.BackOffice.Controllers;
using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Controllers;
public class uSyncMigrationsController : UmbracoAuthorizedApiController
{
    private readonly MigrationService _migrationService;

    public uSyncMigrationsController(MigrationService migrationService)
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
    public bool HasPendingMigration()
        => true;

    [HttpGet]
    public UserMigrationOptions GetMigrationOptions()
    {
        return new UserMigrationOptions
        {
            HasPending = true,
            Handlers = _migrationService.HandlerTypes()
                .Select(x => new HandlerOption
                {
                    Name = x,
                    Include = false
                })
        };
    }


    [HttpPost]
    public MigrationResults Migrate(MigrationOptions options)
    {
        return _migrationService.MigrateFiles(options);
    }
      
}

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class UserMigrationOptions
{
    public bool HasPending { get; set; }

    public IEnumerable<HandlerOption> Handlers { get; set; }
}

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class HandlerOption
{
    public string Name { get; set; }
    public bool Include { get; set; } = true;
}