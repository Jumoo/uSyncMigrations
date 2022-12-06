using System.Xml.Linq;

using Examine.Lucene.Providers;

using Microsoft.AspNetCore.Hosting;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;

using uSync.BackOffice;
using uSync.BackOffice.Configuration;
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
    private readonly uSyncConfigService _uSyncConfig;
    private readonly string _uSyncRoot;

    public SyncMigrationFileService(
        IWebHostEnvironment webHostEnvironment,
        IHostingEnvironment hostingEnvironment,
        uSyncService uSyncService,
        SyncFileService syncFileService,
        uSyncConfigService uSyncConfig)
    {
        _webHostEnvironment = webHostEnvironment;
        _migrationRoot = Path.Combine(hostingEnvironment.LocalTempPath, "uSync", "Migrations");
        _uSyncService = uSyncService;

        _syncFileService = syncFileService;
        _uSyncConfig = uSyncConfig;

        // gets us the folder above where uSync saves stuff (usually uSync/v9 so this returns uSync); 
        var uSyncPhysicalPath = _webHostEnvironment.MapPathContentRoot(_uSyncConfig.GetRootFolder()).TrimEnd(Path.DirectorySeparatorChar);
        _uSyncRoot = Path.GetDirectoryName(uSyncPhysicalPath) ?? _webHostEnvironment.MapPathContentRoot("uSync");
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
            throw new AccessViolationException("Cannot migrate outside the uSync folder");

        return path;
    }

    static Dictionary<int, string[]> _wellKnownPaths = new Dictionary<int, string[]>()
    {
        { 7, new [] { "DataType", "DocumentType" } },
        { 8, new [] { "DataTypes", "ContentTypes"} }
    };

    public Attempt<string> ValdateMigrationSource(int version, string folder)
    {
        var path = _syncFileService.GetAbsPath(folder);

        if (!Directory.Exists(path))
        {
            return Attempt<string>.Fail(new DirectoryNotFoundException($"Root folder '{path}' doesn't exist"));
        }

        foreach (var expectedFolder in _wellKnownPaths[version])
        {
            if (!Directory.Exists(Path.Combine(path, expectedFolder)))
                return Attempt<string>.Fail(new DirectoryNotFoundException($"Missing well known folder '{expectedFolder}'"));
        }

        return Attempt<string>.Succeed("Pass");
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
