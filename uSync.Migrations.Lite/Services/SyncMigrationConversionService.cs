using uSync.BackOffice.Services;
using uSync.Core;
using uSync.Migrations.Core;

namespace uSync.Migrations.Lite.Services;
internal class SyncMigrationConversionService : ISyncMigrationConversionService
{
    private readonly SyncFileService _fileService;

    public SyncMigrationConversionService(SyncFileService fileService)
    {
        _fileService = fileService;
    }
    private static string[] _legacyEditors = new string[] {
        "Umbraco.Grid",
        uSyncMigrations.EditorAliases.NestedContent
    };

    public bool HasLegacyEditors()
    {
        var dataTypes = _fileService.GetFiles("~/uSync/v9/DataTypes/", "*.config");

        foreach (var file in dataTypes)
        {
            var node = _fileService.LoadXElement(file);

            var editorAlias = node.Element("Info")?
                .Element("EditorAlias")?
                .ValueOrDefault(string.Empty);

            if (_legacyEditors.Contains(editorAlias)) return true;
        }

        return false;

    }
}
