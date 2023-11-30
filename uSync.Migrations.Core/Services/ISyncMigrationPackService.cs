using uSync.BackOffice;

namespace uSync.Migrations.Core.Services;
public interface ISyncMigrationPackService
{
    Guid CreateSitePack(Guid id, uSyncCallbacks? callbacks);
    FileStream ZipPack(Guid id);
}