using Umbraco.Extensions;

using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Context;
using uSync.Migrations.Services;
using uSync.Migrations.Models;

namespace uSync.Migrations.Validation;

internal class FileStructureValidator : ISyncMigrationValidator
{
    private readonly ISyncMigrationFileService _migrationFileService;

    public FileStructureValidator(ISyncMigrationFileService migrationFileService)
    {
        _migrationFileService = migrationFileService;
    }

    public IEnumerable<MigrationMessage> Validate(SyncValidationContext validationContext)
    {
        var result = _migrationFileService.ValdateMigrationSource(validationContext.Metadata.SourceVersion, validationContext.Options.Source);

        var message = new MigrationMessage("FileValidator", "Files",
            result.Success ? MigrationMessageType.Success : MigrationMessageType.Error);

        if (!result.Success)
        {
            message.Message = result.Exception?.Message ?? result.Result ?? "Unknown Error";
        }
        else
        {
            message.Message = $"File structure looks like it's Umbraco {validationContext.Metadata.SourceVersion} 🤷";
        }

        return message.AsEnumerableOfOne();
    }
}
