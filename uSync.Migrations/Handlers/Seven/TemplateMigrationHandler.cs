using System.Xml.Linq;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;

using uSync.Core;
using uSync.Migrations.Configuration;
using uSync.Migrations.Context;
using uSync.Migrations.Handlers.Shared;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Seven;

[SyncMigrationHandler(BackOfficeConstants.Groups.Settings, uSyncMigrations.Priorities.Templates,
    SourceVersion = 7,
    SourceFolderName = "Template",
    TargetFolderName = "Templates")]
internal class TemplateMigrationHandler : SharedTemplateHandler,  ISyncMigrationHandler
{
    public TemplateMigrationHandler(
        IOptions<uSyncMigrationOptions> options,
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        IFileService fileService,
        ILogger<TemplateMigrationHandler> logger)
        : base(options,eventAggregator, migrationFileService, fileService, logger)
    { }

    protected override XElement? MigrateFile(XElement source, int level, SyncMigrationContext context)
    {
        var key = source.Element("Key").ValueOrDefault(Guid.Empty);
        var alias = source.Element("Alias").ValueOrDefault(string.Empty);
        var name = source.Element("Name").ValueOrDefault(string.Empty);
        var master = source.Element("Master").ValueOrDefault(string.Empty);

        var target = new XElement("Template",
            new XAttribute(uSyncConstants.Xml.Key, key),
            new XAttribute(uSyncConstants.Xml.Alias, alias),
            new XAttribute(uSyncConstants.Xml.Level, level),
            new XElement("Name", name),
            new XElement("Parent", string.IsNullOrEmpty(master) ? null : master));

        return target;
    }

    protected override (string alias, Guid key) GetAliasAndKey(XElement source)
        => (
            alias: source.Element("Alias").ValueOrDefault(string.Empty),
            key: source.Element("Key").ValueOrDefault(Guid.Empty)
        );
    
}
