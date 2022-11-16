using System.Xml.Linq;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;

using uSync.Core;
using uSync.Migrations.Models;
using uSync.Migrations.Notifications;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;

internal class TemplateMigrationHandler : ISyncMigrationHandler
{
    private readonly IEventAggregator _eventAggregator;
    private readonly ISyncMigrationFileService _migrationFileService;

    public TemplateMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService)
    {
        _eventAggregator = eventAggregator;
        _migrationFileService = migrationFileService;
    }

    public string Group => uSync.BackOffice.uSyncConstants.Groups.Settings;

    public string ItemType => nameof(Template);

    public int Priority => uSyncMigrations.Priorities.Templates;

    public void PrepareMigrations(Guid migrationId, string sourceFolder, SyncMigrationContext context)
    {
        var folder = Path.Combine(sourceFolder, "Template");

        if (Directory.Exists(folder) == false)
        {
            return;
        }

        var files = Directory.GetFiles(folder, "*.config").ToList();

        foreach (var file in files)
        {
            var source = XElement.Load(file);
            var (alias, key) = GetAliasAndKey(source);
            context.AddTemplateKey(alias, key);
        }
    }

    public IEnumerable<MigrationMessage> MigrateFromDisk(Guid migrationId, string sourceFolder, SyncMigrationContext context)
    {
        return MigrateFolder(migrationId, Path.Combine(sourceFolder, "Template"), 0, context);
    }

    private IEnumerable<MigrationMessage> MigrateFolder(Guid id, string folder, int level, SyncMigrationContext context)
    {
        if (Directory.Exists(folder) == false)
        {
            return Enumerable.Empty<MigrationMessage>();
        }

        var files = Directory.GetFiles(folder, "*.config").ToList();

        var messages = new List<MigrationMessage>();

        foreach (var file in files)
        {
            var source = XElement.Load(file);
            var (alias, key) = GetAliasAndKey(source);

            context.AddTemplateKey(alias, key);

            var migratingNotification = new SyncMigratingNotification<Template>(source, context);
            if (_eventAggregator.PublishCancelable(migratingNotification) == true)
            {
                continue;
            }

            if (context.IsBlocked(ItemType, alias)) continue;

            var target = ConvertTemplate(source, level);

            if (target != null)
            {
                var migratedNotification = new SyncMigratedNotification<Template>(target, context).WithStateFrom(migratingNotification);
                _eventAggregator.Publish(migratedNotification);
                messages.Add(SaveTargetXml(id, target));
            }
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
            new XElement("Parent", string.IsNullOrEmpty(master) ? null : master));

        return target;
    }

    private (string alias, Guid key) GetAliasAndKey(XElement source)
    {
        return (
            alias : source.Element("Alias").ValueOrDefault(string.Empty),
            key : source.Element("Key").ValueOrDefault(Guid.Empty)
        );
    }

    private MigrationMessage SaveTargetXml(Guid id, XElement xml)
    {
        _migrationFileService.SaveMigrationFile(id, xml, "Templates");

        return new MigrationMessage(ItemType, xml.GetAlias(), MigrationMessageType.Success);
    }
}
