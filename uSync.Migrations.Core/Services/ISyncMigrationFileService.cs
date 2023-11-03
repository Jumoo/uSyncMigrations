using System.Xml.Linq;

using Umbraco.Cms.Core;

using uSync.Migrations.Core.Models;

namespace uSync.Migrations.Core.Services;

public interface ISyncMigrationFileService
{
    void CopyMigrationToFolder(Guid id, string targetFolder);
    void DeleteMigration(string migrationId);
    string GetMigrationFolder(string folder, bool clean);
    IEnumerable<MigrationStatus> GetMigrations();
    string GetWebSitePath(string folder);
    void RemoveMigration(Guid migrationId);
    void SaveMigrationFile(Guid id, XElement xml, string folder);
    Attempt<string> ValdateMigrationSource(int version, string folder);
}