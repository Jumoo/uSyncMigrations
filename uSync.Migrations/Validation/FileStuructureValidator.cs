using Umbraco.Extensions;

using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Validation;

internal class FileStuructureValidator : ISyncMigrationValidator
{
    private readonly ISyncMigrationFileService _migrationFileService;

    public FileStuructureValidator(ISyncMigrationFileService migrationFileService)
    {
        _migrationFileService = migrationFileService;
    }

    public IEnumerable<MigrationMessage> Validate(MigrationOptions options)
    {
        var result = _migrationFileService.ValdateMigrationSource(options.Source);

        var message = new MigrationMessage("FileValidator", "Files",
            result.Success ? MigrationMessageType.Success : MigrationMessageType.Error);

        if (!result.Success)
        {
            message.Message = result.Exception?.Message ?? result.Result ?? "Unknown Error";
        }
        else
        {
            message.Message = "File structure looks like its Umbraco 7 🤷";
        }

        return message.AsEnumerableOfOne();
    }
}
