using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Umbraco.Extensions;

using uSync.BackOffice.Services;
using uSync.Migrations.Core.Context;
using uSync.Migrations.Core.Models;
using uSync.Migrations.Core.Services;

namespace uSync.Migrations.Core.Validation;
internal class XMLValidator : ISyncMigrationValidator
{
    private readonly SyncFileService _fileService;

    public XMLValidator(SyncFileService fileService)
    {
        _fileService = fileService;
    }

    public IEnumerable<MigrationMessage> Validate(SyncValidationContext validationContext)
    {
        var folder = _fileService.GetAbsPath(validationContext.Metadata.SourceFolder);
        var files = Directory.GetFiles(folder, "*.config", SearchOption.AllDirectories);

        var errors = new List<MigrationMessage>();

        foreach (var file in files) 
        { 
            try
            {
                var node = XElement.Load(file);
            }
            catch
            {
                var message = new MigrationMessage("XML", "XML Validation", MigrationMessageType.Error);
                var relativeFileName = file.Substring(validationContext.Metadata.SourceFolder.Length);
                message.Message += $"Failed: {relativeFileName} is not valid xml";
                errors.Add(message);
            }
        }

        if (errors.Count > 0)
            return errors;

        return new MigrationMessage("XML", "XML Validation", MigrationMessageType.Success)
        {
            Message = "All uSync files in the source folder appear to be valid xml"
        }.AsEnumerableOfOne();
        
    }
}
