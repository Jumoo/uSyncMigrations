using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Models;

namespace uSync.Migrations.Services;

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