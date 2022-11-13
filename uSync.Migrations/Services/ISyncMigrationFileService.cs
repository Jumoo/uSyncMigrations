using System.Xml.Linq;

namespace uSync.Migrations.Services;
public interface ISyncMigrationFileService
{
    void CopyMigrationToFolder(Guid id, string targetFolder);
    string GetMigrationFolder(string folder);
    void RemoveMigration(Guid migrationId);
    void SaveMigrationFile(Guid id, XElement xml, string folder);
}