using Microsoft.AspNetCore.Hosting;
using System.Xml.Linq;
using Umbraco.Cms.Core.Extensions;
using uSync.BackOffice;
using uSync.Core;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace uSync.Migrations.Services;

public class SyncMigrationFileService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly string _migrationRoot;
    private readonly uSyncService _uSyncService;

    public SyncMigrationFileService(
        IWebHostEnvironment webHostEnvironment,
        IHostingEnvironment hostingEnvironment,
        uSyncService uSyncService)
    {
        _webHostEnvironment = webHostEnvironment;
        _migrationRoot = Path.Combine(hostingEnvironment.LocalTempPath, "uSync", "Migrations");
        _uSyncService = uSyncService;
    }

    public void SaveMigrationFile(Guid id, XElement xml, string folder)
    {
        var filename = $"{xml.GetKey()}.config";

        var directory = GetMigrationFolder(id, folder);
        Directory.CreateDirectory(directory);

        var fullPath = Path.Combine(directory, filename);
        xml.Save(fullPath);
    }

    public void CopyMigrationToFolder(Guid id, string targetFolder)
        => _uSyncService.ReplaceFiles(GetMigrationFolder(id), targetFolder, false); // NOTE: `true` = clean/delete existing

    public string GetMigrationSource(string folder)
        => Path.Combine(_webHostEnvironment.MapPathContentRoot("~/uSync/"), folder);

    private string GetMigrationFolder(Guid id)
        => Path.Combine(_migrationRoot, id.ToString());

    private string GetMigrationFolder(Guid id, string folder)
        => Path.Combine(GetMigrationFolder(id), folder);
}
