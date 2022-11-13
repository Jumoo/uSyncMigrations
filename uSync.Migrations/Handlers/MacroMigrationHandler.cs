using System.Xml.Linq;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;

using uSync.Core;
using uSync.Migrations.Models;
using uSync.Migrations.Notifications;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;

internal class MacroMigrationHandler : ISyncMigrationHandler
{
    private readonly IEventAggregator _eventAggregator;
    private readonly ISyncMigrationFileService _migrationFileService;

    public MacroMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService)
    {
        _eventAggregator = eventAggregator;
        _migrationFileService = migrationFileService;
    }

    public string Group => uSync.BackOffice.uSyncConstants.Groups.Settings;

    public string ItemType => nameof(Macro);

    public int Priority => uSyncMigrations.Priorities.Macros;

    public void PrepareMigrations(Guid migrationId, string sourceFolder, SyncMigrationContext context)
    { }

    public IEnumerable<MigrationMessage> MigrateFromDisk(Guid migrationId, string sourceFolder, SyncMigrationContext context)
    {
        var macroFolder = Path.Combine(sourceFolder, ItemType);

        if (Directory.Exists(macroFolder) == false)
        {
            return Enumerable.Empty<MigrationMessage>();
        }

        var messages = new List<MigrationMessage>();

        foreach (var file in Directory.GetFiles(macroFolder, "*.config", SearchOption.AllDirectories))
        {
            var source = XElement.Load(file);

            var migratingNotification = new SyncMigratingNotification<Macro>(source, context);

            if (_eventAggregator.PublishCancelable(migratingNotification) == true)
            {
                continue;
            }

            var target = MigrateMacro(source);

            if (target != null)
            {
                var migratedNotification = new SyncMigratedNotification<Macro>(target, context).WithStateFrom(migratingNotification);

                _eventAggregator.Publish(migratedNotification);

                messages.Add(SaveTargetXml(migrationId, target));
            }
        }

        return messages;
    }

    private XElement MigrateMacro(XElement source)
    {
        var key = source.Element("Key").ValueOrDefault(Guid.Empty);
        var alias = source.Element("alias").ValueOrDefault(string.Empty);

        var target = new XElement("Macro",
            new XAttribute(uSyncConstants.Xml.Key, key),
            new XAttribute(uSyncConstants.Xml.Alias, alias),
            new XAttribute(uSyncConstants.Xml.Level, 0));

        target.Add(new XElement("Name", source.Element("name").ValueOrDefault(alias)));
        target.Add(new XElement("MacroSource", source.Element("scriptingFile").ValueOrDefault(string.Empty)));
        target.Add(new XElement("UseInEditor", source.Element("useInEditor").ValueOrDefault(false)));
        target.Add(new XElement("DontRender", source.Element("dontRender").ValueOrDefault(false)));
        target.Add(new XElement("CachedByMember", source.Element("cacheByMember").ValueOrDefault(false)));
        target.Add(new XElement("CachedByPage", source.Element("cacheByPage").ValueOrDefault(false)));
        target.Add(new XElement("CachedDuration", source.Element("refreshRate").ValueOrDefault(0)));

        var sourceProperties = source.Element("properties");
        if (sourceProperties != null)
        {
            var properties = new XElement("Properties");

            foreach (var property in sourceProperties.Elements("property"))
            {
                var propertyType = property.Attribute("propertyType").ValueOrDefault(string.Empty);

                var newProperty = new XElement("Property",
                    new XElement("Name", property.Attribute("name").ValueOrDefault(string.Empty)),
                    new XElement("Alias", property.Attribute("alias").ValueOrDefault(string.Empty)),
                    new XElement("SortOrder", property.Attribute("sortOrder").ValueOrDefault(0)),
                    new XElement("EditorAlias", MapPropertyType(propertyType)));

                properties.Add(newProperty);
            }

            target.Add(properties);
        }

        return target;
    }

    private static Dictionary<string, string> _mappedTypes = new()
    {
        { "Umbraco.ContentPicker2", UmbConstants.PropertyEditors.Aliases.ContentPicker },
        { "Umbraco.MediaPicker2", UmbConstants.PropertyEditors.Aliases.MediaPicker },
        { "Umbraco.ContentPickerAlias", UmbConstants.PropertyEditors.Aliases.ContentPicker }
    };

    private string MapPropertyType(string editorAlias)
    {
        if (_mappedTypes.ContainsKey(editorAlias) == true)
        {
            return _mappedTypes[editorAlias];
        }

        return editorAlias;
    }

    private MigrationMessage SaveTargetXml(Guid id, XElement xml)
    {
        _migrationFileService.SaveMigrationFile(id, xml, "Macros");

        return new MigrationMessage(ItemType, xml.GetAlias(), MigrationMessageType.Success);
    }
}
