using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Strings;

using uSync.Core;
using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;
internal class TemplateMigrationHandler : ISyncMigrationHandler
{
    private readonly MigrationFileService _migrationFileService;
    private ILogger<TemplateMigrationHandler> _logger;

    public TemplateMigrationHandler(
        ILogger<TemplateMigrationHandler> logger,
        MigrationFileService migrationFileService)
    {
        _logger = logger;
        _migrationFileService = migrationFileService;
    }


    public int Priority => uSyncMigrations.Priorities.Templates;
    public string ItemType => "Template";

    public IEnumerable<MigrationMessage> MigrateFromDisk(Guid migrationId, string sourceFolder, MigrationContext context)
    {
        return MigrateFolder(migrationId, Path.Combine(sourceFolder, "Template"), 0, context);
    }

    private IEnumerable<MigrationMessage> MigrateFolder(Guid id, string folder, int level, MigrationContext context)
    {
        if (!Directory.Exists(folder)) return Enumerable.Empty<MigrationMessage>();

        var files = Directory.GetFiles(folder, "*.config").ToList();

        var messages = new List<MigrationMessage>();

        foreach (var file in files)
        {
            var source = XElement.Load(file);

            context.AddTemplateKey(
                source.Element("Alias").ValueOrDefault(string.Empty),
                source.Element("Key").ValueOrDefault(Guid.Empty));

            var target = ConvertTemplate(source, level);

            messages.Add(SaveTargetXml(id, target));
        }

        var folders = Directory.GetDirectories(folder);
        foreach (var childFolder in folders)
        {
            messages.AddRange(MigrateFolder(id, childFolder, level + 1, context));
        }

        return messages;

    }

    private XElement ConvertTemplate(XElement source, int level)
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
            new XElement("Parent", master));

        return target;
    }


    private MigrationMessage SaveTargetXml(Guid id, XElement xml)
    {
        _migrationFileService.SaveMigrationFile(id, xml, "Templates");
        return new MigrationMessage(ItemType, xml.GetAlias(), MigrationMessageType.Success);
    }

    public void PrepMigrations(Guid migrationId, string sourceFolder, MigrationContext context)
    { }
}
