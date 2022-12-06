using System.Xml.Linq;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

using uSync.Core;
using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;

[SyncMigrtionHandler(BackOfficeConstants.Groups.Settings, uSyncMigrations.Priorities.Templates, 7,
    SourceFolderName = "Template",
    TargetFolderName = "Templates")]
internal class TemplateMigrationHandler : MigrationHandlerBase<Template>,  ISyncMigrationHandler
{
    private readonly IFileService _fileService;

    public TemplateMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        IFileService fileService)
        : base(eventAggregator, migrationFileService)
    {
        _fileService = fileService;
    }

    public override void Prepare(SyncMigrationContext context)
    {
        foreach (var template in _fileService.GetTemplates())
        {
            context.AddTemplateKey(template.Alias, template.Key);
        }
    }

    protected override void PrepareFile(XElement source, SyncMigrationContext context)
    {
        var (alias, key) = GetAliasAndKey(source);
        context.AddTemplateKey(alias, key);
    }

    protected override XElement? MigrateFile(XElement source, int level, SyncMigrationContext context)
    {
        var (alias, _) = GetAliasAndKey(source);

        if (context.IsBlocked(ItemType, alias)) return null;
        return ConvertTemplate(source, level);
    }

    private static XElement ConvertTemplate(XElement source, int level)
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

    private static (string alias, Guid key) GetAliasAndKey(XElement source)
    {
        return (
            alias: source.Element("Alias").ValueOrDefault(string.Empty),
            key: source.Element("Key").ValueOrDefault(Guid.Empty)
        );
    }
}
