using System.Xml.Linq;

using Microsoft.AspNetCore.Hosting;

using Umbraco.Cms.Core.Extensions;

using uSync.BackOffice;
using uSync.BackOffice.Services;
using uSync.Core;

using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace uSync.Migrations.Services;

internal class SyncMigrationFileService : ISyncMigrationFileService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly string _migrationRoot;
    private readonly uSyncService _uSyncService;
    private readonly SyncFileService _syncFileService;

    private readonly string _uSyncRoot;

    public SyncMigrationFileService(
        IWebHostEnvironment webHostEnvironment,
        IHostingEnvironment hostingEnvironment,
        uSyncService uSyncService,
        SyncFileService syncFileService)
    {
        _webHostEnvironment = webHostEnvironment;
        _migrationRoot = Path.Combine(hostingEnvironment.LocalTempPath, "uSync", "Migrations");
        _uSyncService = uSyncService;

        _uSyncRoot = _webHostEnvironment.MapPathContentRoot("~/uSync");
        _syncFileService = syncFileService;
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

    public string GetMigrationFolder(string folder)
    {
        var path = Path.Combine(_webHostEnvironment.MapPathContentRoot(folder));
        if (!path.StartsWith(_uSyncRoot, StringComparison.OrdinalIgnoreCase))
            throw new AccessViolationException("Cannot migrate outsided the uSync folder");

        return path;
    }


    private string GetMigrationFolder(Guid id)
        => Path.Combine(_migrationRoot, id.ToString());

    private string GetMigrationFolder(Guid id, string folder)
        => Path.Combine(GetMigrationFolder(id), folder);

    public void RemoveMigration(Guid migrationId)
    {
        var path = Path.Combine(GetMigrationFolder(migrationId));
        _syncFileService.DeleteFolder(path, true);
    }
}
