using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Web.BackOffice.Controllers;

using uSync.Migrations.Handlers;
using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Controllers;
public class MigrationTestController : UmbracoAuthorizedApiController
{
    private readonly IHostingEnvironment _hostingEnvironment;

    private readonly MigrationHandlerCollection _migrationHandlers;

    public MigrationTestController(
        IHostingEnvironment hostingEnvironment,
        MigrationHandlerCollection migrationHandlers)
    {
        _hostingEnvironment = hostingEnvironment;
        _migrationHandlers = migrationHandlers;
    }

    public bool GetApi() => true;

    public IEnumerable<MigrationMessage> Migrate()
    {
        var id = Guid.NewGuid();
        var sourceRoot = _hostingEnvironment.MapPathContentRoot("~/uSync/data");

        var handlers = _migrationHandlers.OrderBy(x => x.Priority);
        var context = new MigrationContext();

        var results = new List<MigrationMessage>();

        foreach(var handler in handlers)
        {
            results.AddRange(handler.MigrateFromDisk(id, sourceRoot, context));
        }

        return results;
    }
}
