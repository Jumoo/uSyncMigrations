using System.Xml.Linq;

using Microsoft.AspNetCore.Hosting;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;

using uSync.BackOffice;
using uSync.BackOffice.Configuration;
using uSync.BackOffice.Services;
using uSync.Core;
using uSync.Migrations.Helpers;
using uSync.Migrations.Models;

using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace uSync.Migrations.Services;

internal class SyncMigrationFileService : ISyncMigrationFileService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly string _migrationRoot;
    private readonly uSyncService _uSyncService;
    private readonly SyncFileService _syncFileService;
    private readonly uSyncConfigService _uSyncConfig;
    private readonly string _uSyncRoot;

    private readonly ISyncMigrationStatusService _migrationStatusService;

    public SyncMigrationFileService(
        IWebHostEnvironment webHostEnvironment,
        IHostingEnvironment hostingEnvironment,
        uSyncService uSyncService,
        SyncFileService syncFileService,
        uSyncConfigService uSyncConfig,
        ISyncMigrationStatusService migrationStatusService)
    {
        _webHostEnvironment = webHostEnvironment;
        _migrationRoot = Path.Combine(hostingEnvironment.LocalTempPath, "uSync", "Migrations");
        _uSyncService = uSyncService;

        _syncFileService = syncFileService;
        _uSyncConfig = uSyncConfig;

        // gets us the folder above where uSync saves stuff (usually uSync/v9 so this returns uSync); 
        var uSyncPhysicalPath = _webHostEnvironment.MapPathContentRoot(_uSyncConfig.GetRootFolder()).TrimEnd(Path.DirectorySeparatorChar);
        _uSyncRoot = Path.GetDirectoryName(uSyncPhysicalPath) ?? _webHostEnvironment.MapPathContentRoot("uSync");
        _migrationStatusService = migrationStatusService;
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

    public string GetMigrationFolder(string folder, bool clean)
    {
        var path = Path.Combine(_webHostEnvironment.MapPathContentRoot(folder));
        if (!path.StartsWith(_uSyncRoot, StringComparison.OrdinalIgnoreCase))
            throw new AccessViolationException("Cannot migrate outside the uSync folder");

        if (clean && Directory.Exists(path))
        {
            try
            {
                Directory.Delete(path, true);
            }
            catch { }
        }
           

        Directory.CreateDirectory(path);

        var attempt = MigrationIoHelpers.FinduSyncPath(path);
        if (attempt.Success) return attempt.Result ?? path;

        return path;
    }

   


    public Attempt<string> ValdateMigrationSource(int version, string folder)
    {
        var path = _syncFileService.GetAbsPath(folder);

        if (!Directory.Exists(path))
        {
            return Attempt<string>.Fail(new DirectoryNotFoundException($"Root folder '{path}' doesn't exist"));
        }

        return MigrationIoHelpers.CheckForFolders(path, MigrationIoHelpers.WellKnownPaths[version]);
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


    public IEnumerable<MigrationStatus> GetMigrations()
        => _migrationStatusService.GetAll();

    public void DeleteMigration(string migrationId)
    {
        var migration = GetMigrations()
            .FirstOrDefault(x => x.Id != null && x.Id.Equals(migrationId));

        if (migration != null && migration.Root != null)
        {
            var fullpath = Path.Combine(_webHostEnvironment.ContentRootPath, migration.Root.TrimStart(Path.DirectorySeparatorChar));
            if (Directory.Exists(fullpath))
            {
                Directory.Delete(fullpath, true);
            }
        }
    }
}
