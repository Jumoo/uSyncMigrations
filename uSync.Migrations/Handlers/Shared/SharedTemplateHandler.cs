using System.Xml.Linq;

using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using uSync.Migrations.Context;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Shared;

/// <summary>
///  stuff that is the same in v7 and v8 template migrations.
/// </summary>
internal abstract class SharedTemplateHandler : SharedHandlerBase<Template>
{
    protected readonly IFileService _fileService;

    protected SharedTemplateHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        IFileService fileService,
        ILogger<SharedTemplateHandler> logger) 
        : base(eventAggregator, migrationFileService, logger)
    {
        _fileService = fileService;
    }

    public override void Prepare(SyncMigrationContext context)
    {
        _fileService.GetTemplates().ToList()
            .ForEach(template => context.Templates.AddAlias(template.Alias, template.Key));
    }

    protected override void PrepareFile(XElement source, SyncMigrationContext context)
    {
        var (alias, key) = GetAliasAndKey(source);
        context.Templates.AddAlias(alias, key);
    }
}
