using uSync.Migrations.Core.Configuration.Models;
using uSync.Migrations.Core.Models;

namespace uSync.Migrations.Core.Services;

public interface ISyncMigrationStatusService
{
    MigrationOptions? ConvertToOptions(MigrationStatus status, MigrationOptions defaultOptions);
    MigrationStatus? CreateNew(MigrationStatus status);
    MigrationStatus? CreateStatus(string folder);
    MigrationStatus? Get(string id);
    IEnumerable<MigrationStatus> GetAll();
    string? GetDefaultProfile(int version);
    MigrationStatus? LoadStatus(string folder);
    void SaveStatus(string? folder, MigrationStatus status);
}