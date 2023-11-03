using Umbraco.Extensions;

using uSync.Migrations.Core.Services;

namespace uSync.Migrations.Migrators.Validation;

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
