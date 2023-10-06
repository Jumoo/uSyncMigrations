using System.Xml.Linq;

using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

using uSync.Core;
using uSync.Migrations.Context;
using uSync.Migrations.Handlers.Shared;
using uSync.Migrations.Models;
using uSync.Migrations.Notifications;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Seven;

[SyncMigrationHandler(BackOfficeConstants.Groups.Settings, uSyncMigrations.Priorities.Dictionary, 
    SourceVersion = 7,
    SourceFolderName = "DictionaryItem",
    TargetFolderName = "Dictionary")]
internal class DictionaryMigrationHandler : SharedHandlerBase<DictionaryItem>, ISyncMigrationHandler
{
    public DictionaryMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        ILogger<DictionaryMigrationHandler> logger)
        : base(eventAggregator, migrationFileService, logger)
    { }

    /// <summary>
    ///  not actually called for a dictionary item, see DoMigration below.
    /// </summary>
    protected override XElement? MigrateFile(XElement source, int level, SyncMigrationContext context)
         => null;

    protected override (string alias, Guid key) GetAliasAndKey(XElement source, SyncMigrationContext? context)
        => (
            alias: source.Attribute("Key").ValueOrDefault(string.Empty),
            key: source.Attribute("guid").ValueOrDefault(Guid.Empty)
        );

    /// <summary>
    ///  dictionary splits a single XElement into multiple, so it is slightly diferent from the rest.
    /// </summary>
    public override IEnumerable<MigrationMessage> DoMigration(SyncMigrationContext context)
    {
        var files = GetSourceFiles(context.Metadata.SourceFolder);
        if (files == null) { 
            return Enumerable.Empty<MigrationMessage>();
        }

        var messages = new List<MigrationMessage>();

        foreach (var file in files)
        {
            var source = XElement.Load(file);

            var migratingNotification = new SyncMigratingNotification<DictionaryItem>(source, context);
            if (_eventAggregator.PublishCancelable(migratingNotification) == true)
            {
                continue;
            }

            var targets = MigrateDictionary(source, string.Empty, 0).ToList();

            if (targets != null && targets.Count > 0)
            {
                foreach (var target in targets)
                {
                    var migratedNotification = new SyncMigratedNotification<DictionaryItem>(target, context).WithStateFrom(migratingNotification);
                    _eventAggregator.Publish(migratedNotification);
                    messages.Add(SaveTargetXml(context.Metadata.MigrationId, target));
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

        foreach (var value in childSource.Elements("Value"))
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
            nodes.AddRange(MigrateDictionary(child, alias, level + 1));
        }

        return nodes;
    }
}
