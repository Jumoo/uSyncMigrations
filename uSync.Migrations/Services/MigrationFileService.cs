using System.Xml.Linq;

using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

using uSync.BackOffice;
using uSync.Core;

namespace uSync.Migrations.Services;
public class MigrationFileService
{
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly string _migrationRoot;
    private readonly IShortStringHelper _shortStringHelper;

    private readonly uSyncService _uSyncService;


    public MigrationFileService(
        IHostingEnvironment hostingEnvironment,
        IShortStringHelper shortStringHelper,
        uSyncService uSyncService)
    {
        _hostingEnvironment = hostingEnvironment;
        _migrationRoot = Path.Combine(hostingEnvironment.LocalTempPath,
            "uSync", "Migrations");
        _shortStringHelper = shortStringHelper;
        _uSyncService = uSyncService;
    }

    public void SaveMigrationFile(Guid id, XElement xml, string folder)
    {

        var filename = xml.GetAlias().ToSafeFileName(_shortStringHelper) + ".config"; 
        // var filename = xml.GetKey().ToString() + ".config";
        var directory = GetMigrationFolder(id, folder);
        Directory.CreateDirectory(directory);
        var fullPath = Path.Combine(directory, filename);
        xml.Save(fullPath);
    }

    public void CopyMigrationToFolder(Guid id, string targetFolder)
    {
        _uSyncService.ReplaceFiles(GetMigrationFolder(id), targetFolder, true);
    }

    public string GetMigrationSource(string folder)
        => Path.Combine(_hostingEnvironment.MapPathContentRoot("~/uSync/"), folder);

    private string GetMigrationFolder(Guid id)
        => Path.Combine(_migrationRoot, id.ToString());

    private string GetMigrationFolder(Guid id, string folder)
        => Path.Combine(GetMigrationFolder(id), folder);
}
