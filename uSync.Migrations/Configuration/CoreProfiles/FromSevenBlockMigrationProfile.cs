using uSync.Migrations.Composing;
using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Migrators.Optional;

namespace uSync.Migrations.Configuration.CoreProfiles;

internal class FromSevenBlockMigrationProfile : ISyncMigrationProfile
{
    private readonly SyncMigrationHandlerCollection _migrationHandlers;

    public FromSevenBlockMigrationProfile(SyncMigrationHandlerCollection migrationHandlers)
    {
        _migrationHandlers = migrationHandlers;
    }

    public int Order => 200;

    public string Name => "7 O'Block";

    public string Icon => "icon-brick color-green";

    public string Description => "Convert to block list (Experimental!)";

    public MigrationOptions Options => new MigrationOptions
    {
        Source = "uSync/v9",
        Target = $"{uSyncMigrations.MigrationFolder}/{DateTime.Now:yyyyMMdd_HHmmss}",
        Handlers = _migrationHandlers.SelectGroup(7, string.Empty),
        SourceVersion = 7,
        PreferredMigrators = new Dictionary<string, string>
        {
            { Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.Grid, nameof(GridToBlockGridMigrator) },
            { Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.NestedContent, nameof(NestedToBlockListMigrator) }
        }
    };

}