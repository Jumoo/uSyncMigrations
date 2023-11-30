using System.IO.Compression;
using System.Text;

using Microsoft.Extensions.Hosting;

using Newtonsoft.Json;

using Umbraco.Cms.Core.Configuration.Grid;
using Umbraco.Cms.Core.Extensions;

using uSync.BackOffice;
using uSync.BackOffice.SyncHandlers;

namespace uSync.Migrations.Core.Services;


/// <summary>
///  service for creating an exportable pack
///  of site config, content and files. 
/// </summary>
/// <remarks>
///  a migration pack lets us export the 
///  current site setup, so the migration
///  can happen on another site. 
///  
///  we might - just put this in uSync. then
///  it can be an option.
/// </remarks>
internal class SyncMigrationPackService : ISyncMigrationPackService

{
    private const string _uSyncFolder = "v9";
    private const string _siteFolder = "_site";

    private readonly uSyncService _uSyncService;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IGridConfig _gridConfig;

    private readonly string _root;

    public SyncMigrationPackService(
        IHostEnvironment hostEnvironment,
        uSyncService uSyncService,
        IGridConfig gridConfig)
    {
        _hostEnvironment = hostEnvironment;
        _uSyncService = uSyncService;

        _root = _hostEnvironment.MapPathContentRoot("~/uSync/packs");
        _gridConfig = gridConfig;
    }

    public Guid CreateSitePack(Guid id, uSyncCallbacks? callbacks)
    {
        if (id == Guid.Empty)
            id = Guid.NewGuid();

        var folder = Path.Combine(GetPackFolder(id), _uSyncFolder);

        CreateExport(folder, callbacks);

        CopyFiles(id);

        GetConfig(id);

        return id;
    }

    public FileStream ZipPack(Guid id)
    {
        var packFolder = GetPackFolder(id);

        if (Directory.Exists(packFolder) is false)
            throw new DirectoryNotFoundException(nameof(packFolder));

        var filename = $"migration_pack_{DateTime.Now:yyyy_MM_dd_HHmmss}.zip";
        var folderInfo = new DirectoryInfo(packFolder);
        var files = folderInfo.GetFiles("*.*", SearchOption.AllDirectories).ToList();

        using (var stream = new MemoryStream())
        {
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true, Encoding.UTF8))
            {
                foreach (var file in files)
                {
                    var relativePath = file.FullName.Substring(packFolder.Length + 1)
                        .Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    archive.CreateEntryFromFile(file.FullName, relativePath);
                }
            }

            stream.Seek(0, SeekOrigin.Begin);
            var fileStream = new FileStream(Path.Combine(_root, filename), FileMode.Create);
            stream.CopyTo(fileStream);
            fileStream.Seek(0, SeekOrigin.Begin);

            return fileStream;
        }

    }

    private void CreateExport(string folder, uSyncCallbacks? callbacks)
    {
        var options = new SyncHandlerOptions
        {
            Action = HandlerActions.Export,
            Set = "default"
        };

        var result = _uSyncService.Export(folder, options, callbacks);
    }

    private void GetConfig(Guid id)
    {
        var targetFolder = GetPackFolder(id);

        var configJson = JsonConvert.SerializeObject(_gridConfig.EditorsConfig.Editors, Formatting.Indented);
        var configFile = Path.Combine(targetFolder, _siteFolder, "config", "grid.editors.config.js");

        Directory.CreateDirectory(Path.GetDirectoryName(configFile)); 

        File.WriteAllText(configFile, configJson);
    }

    private void CopyFiles(Guid id)
    {
        CopySiteFolder(id, "views");
        CopySiteFolder(id, "wwwroot/css");
        CopySiteFolder(id, "wwwroot/scripts");
    }

    private void CopySiteFolder(Guid guid, string sourceFolder)
    {
        var targetFolder = Path.Combine(GetPackFolder(guid), _siteFolder, sourceFolder);
        SyncMigrationPackService.CopyFolder(_hostEnvironment.MapPathContentRoot(sourceFolder), targetFolder);
    }

    private static void CopyFolder(string sourceFolder, string targetFolder)
    {
        if (Directory.Exists(sourceFolder) is false) return;

        Directory.CreateDirectory(targetFolder);

        foreach (var file in Directory.GetFiles(sourceFolder, "*.*"))
        {
            var targetFile = Path.Combine(targetFolder, Path.GetFileName(file));
            File.Copy(file, targetFile);
        }

        foreach (var folder in Directory.GetDirectories(targetFolder))
        {
            SyncMigrationPackService.CopyFolder(folder, Path.Combine(targetFolder, Path.GetFileName(folder)));
        }
    }


    private string GetPackFolder(Guid id)
        => Path.Combine(_root, id.ToString());
}
