using Microsoft.AspNetCore.Mvc;

using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;

using uSync.Migrations.Lite.Services;

namespace uSync.Migrations.Lite.Controllers;

[PluginController("uSync")]
public class SyncMigrationSimpleController : UmbracoAuthorizedApiController
{
    private readonly ISyncMigrationConversionService _conversionService;

    public SyncMigrationSimpleController(ISyncMigrationConversionService conversionService)
    {
        _conversionService = conversionService;
    }


    [HttpGet]
    public bool GetApi() => true;


    [HttpGet]
    public bool LegacyItemCheck()
        => _conversionService.HasLegacyEditors();
}
