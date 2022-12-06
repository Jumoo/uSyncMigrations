using Umbraco.Extensions;

using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Validation;

internal class FileStructureValidator : ISyncMigrationValidator
{
    private readonly ISyncMigrationFileService _migrationFileService;

    public FileStructureValidator(ISyncMigrationFileService migrationFileService)
    {
        _migrationFileService = migrationFileService;
    }

    public IEnumerable<MigrationMessage> Validate(MigrationOptions options)
    {
        var result = _migrationFileService.ValdateMigrationSource(options.SourceVersion, options.Source);

        var message = new MigrationMessage("FileValidator", "Files",
            result.Success ? MigrationMessageType.Success : MigrationMessageType.Error);

        if (!result.Success)
        {
            message.Message = result.Exception?.Message ?? result.Result ?? "Unknown Error";
        }
        else
        {
            message.Message = $"File structure looks like it's Umbraco {options.SourceVersion} 🤷";
        }

        return message.AsEnumerableOfOne();
    }
}
