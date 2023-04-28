using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Extensions;

using uSync.BackOffice.Configuration;
using uSync.Core;
using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Models;

namespace uSync.Migrations.Validation;
internal class VersionValidator : ISyncMigrationValidator
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public VersionValidator(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public IEnumerable<MigrationMessage> Validate(MigrationOptions options)
    {
        // gets us the folder above where uSync saves stuff (usually uSync/v9 so this returns uSync); 
        var trunkatedPath = options.Source.Substring(_webHostEnvironment.ContentRootPath.Length);

        return new MigrationMessage("Version", "uSync Folder", MigrationMessageType.Success)
        {
            Message = $"{trunkatedPath} contains uSync version {options.SourceVersion} files"
        }.AsEnumerableOfOne();
    }
}
