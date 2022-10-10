using uSync.Migrations.Controllers;

namespace uSync.Migrations.Models;

public class MigrationOptions
{
    public string MigrationType { get; set; }
    public IEnumerable<HandlerOption> Handlers { get; set; }

    public bool BlockListViews { get; set; } = true;

    public bool BlockCommonTypes { get; set; } = true;
}
