using System.Xml.Linq;

using Umbraco.Cms.Core;

namespace uSync.Migrations.Services;

public interface ISyncMigrationFileService
{
    void CopyMigrationToFolder(Guid id, string targetFolder);

    string GetMigrationFolder(string folder);

    void RemoveMigration(Guid migrationId);

    void SaveMigrationFile(Guid id, XElement xml, string folder);

    Attempt<string> ValdateMigrationSource(string folder);
}