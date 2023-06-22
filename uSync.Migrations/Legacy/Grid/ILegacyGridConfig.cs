using uSync.Migrations.Context;

namespace uSync.Migrations.Legacy.Grid;

public interface ILegacyGridConfig 
{
    ILegacyGridEditorsConfig EditorsConfig { get; }

    ILegacyGridEditorsConfig EditorsByContext(SyncMigrationContext context);
    ILegacyGridEditorsConfig EditorsFromFolder(string folder);
}
