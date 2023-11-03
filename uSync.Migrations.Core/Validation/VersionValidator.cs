using Microsoft.AspNetCore.Hosting;

using Umbraco.Extensions;

using uSync.Migrations.Core.Context;
using uSync.Migrations.Core.Models;

namespace uSync.Migrations.Core.Validation;
internal class VersionValidator : ISyncMigrationValidator
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public VersionValidator(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public IEnumerable<MigrationMessage> Validate(SyncValidationContext validationContext)
    {
        // gets us the folder above where uSync saves stuff (usually uSync/v9 so this returns uSync); 
        var truncatedPath = validationContext.Metadata.SourceFolder.Substring(_webHostEnvironment.ContentRootPath.Length);

        return new MigrationMessage("Version", "uSync Folder", MigrationMessageType.Success)
        {
            Message = $"{truncatedPath} contains uSync version {validationContext.Metadata.SourceVersion} files"
        }.AsEnumerableOfOne();
    }
}
