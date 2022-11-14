using System.Xml.Linq;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

using uSync.Core;
using uSync.Migrations.Models;
using uSync.Migrations.Notifications;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;
internal class DictionaryMigrationHandler : ISyncMigrationHandler
{
    private readonly IEventAggregator _eventAggregator;
    private readonly ISyncMigrationFileService _migrationFileService;

    public DictionaryMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService)
    {
        _eventAggregator = eventAggregator;
        _migrationFileService = migrationFileService;
    }

    public string Group => uSync.BackOffice.uSyncConstants.Groups.Settings;
    public string ItemType => nameof(DictionaryItem);
    public int Priority => uSyncMigrations.Priorities.Dictionary;

    public void PrepareMigrations(Guid migrationId, string sourceFolder, SyncMigrationContext context)
    { }

    public IEnumerable<MigrationMessage> MigrateFromDisk(Guid migrationId, string sourceFolder, SyncMigrationContext context)
    {
        var dictionaryFolder = Path.Combine(sourceFolder, "DictionaryItem");

        if (Directory.Exists(dictionaryFolder) == false )
        {
            return Enumerable.Empty<MigrationMessage>();
        }

        var messages = new List<MigrationMessage>();

        foreach(var file in Directory.GetFiles(dictionaryFolder, "*.config", SearchOption.AllDirectories))
        {
            var source = XElement.Load(file);
            var migratingNotification = new SyncMigratingNotification<DictionaryItem>(source, context);

            if (_eventAggregator.PublishCancelable(migratingNotification) == true)
            {
                continue;
            }

            var targets = MigrateDictionary(source, string.Empty, 0).ToList();

            if (targets != null && targets.Count > 0 )
            {
                foreach (var target in targets)
                {
                    var migratedNotification = new SyncMigratedNotification<DictionaryItem>(target, context).WithStateFrom(migratingNotification);
                    _eventAggregator.Publish(migratedNotification);
                    messages.Add(SaveTargetXml(migrationId, target));
                }
            }
        }

        return messages; 
    }


    private IEnumerable<XElement> MigrateDictionary(XElement childSource, string parent, int level)
    {
        var key = childSource.Attribute("guid").ValueOrDefault(Guid.Empty);
        var alias = childSource.Attribute("Key").ValueOrDefault(string.Empty);

        if (string.IsNullOrWhiteSpace(alias)) 
        { 
            return Enumerable.Empty<XElement>();    
        }

        if (key == Guid.Empty)
        {
            key = alias.ToGuid();
        }

        var newNode = new XElement("Dictionary", 
            new XAttribute("Key", key),
            new XAttribute("Alias", alias),
            new XAttribute("Level", level));

        var info = new XElement("Info");
        if (!string.IsNullOrWhiteSpace(parent))
        {
            info.Add(new XElement("Parent", parent));
        }

        newNode.Add(info);

        var translations = new XElement("Translations");

        foreach(var value in childSource.Elements("Value"))
        {
            var language = value.Attribute("LanguageCultureAlias").ValueOrDefault(string.Empty);

            translations.Add(new XElement("Translation",
                new XAttribute("Language", language), new XCData(value.Value)));
        }

        newNode.Add(translations);

        var nodes = new List<XElement>
        {
            newNode
        };

        foreach (var child in childSource.Elements("DictionaryItem"))
        {
            nodes.AddRange(MigrateDictionary(child, alias, level+1));
        }

        return nodes;
    }

    private MigrationMessage SaveTargetXml(Guid id, XElement xml)
    {
        _migrationFileService.SaveMigrationFile(id, xml, "Dictionary");
        return new MigrationMessage(ItemType, xml.GetAlias(), MigrationMessageType.Success);
    }


}
