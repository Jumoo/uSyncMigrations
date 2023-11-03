using uSync.Migrations.Core.Context;

namespace uSync.Migrations.Core.Legacy.Grid;

public interface ILegacyGridConfig
{
    ILegacyGridEditorsConfig EditorsConfig { get; }

    ILegacyGridEditorsConfig EditorsByContext(SyncMigrationContext context);
    ILegacyGridEditorsConfig EditorsFromFolder(string folder);
}
