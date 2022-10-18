using System.Globalization;
using System.Xml.Linq;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using uSync.Core;
using uSync.Migrations.Models;
using uSync.Migrations.Notifications;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;

internal class LanguageMigrationHandler : ISyncMigrationHandler
{
    private readonly IEventAggregator _eventAggregator;
    private readonly SyncMigrationFileService _migrationFileService;

    public LanguageMigrationHandler(
        IEventAggregator eventAggregator,
        SyncMigrationFileService migrationFileService)
    {
        _eventAggregator = eventAggregator;
        _migrationFileService = migrationFileService;
    }

    public string ItemType => nameof(Language);

    public int Priority => uSyncMigrations.Priorities.Languages;

    public void PrepareMigrations(Guid migrationId, string sourceFolder, SyncMigrationContext context)
    { }

    public IEnumerable<MigrationMessage> MigrateFromDisk(Guid migrationId, string sourceFolder, SyncMigrationContext context)
    {
        var languageFolder = Path.Combine(sourceFolder, "Languages");

        if (Directory.Exists(languageFolder) == false)
        {
            return Enumerable.Empty<MigrationMessage>();
        }

        var messages = new List<MigrationMessage>();

        foreach (var file in Directory.GetFiles(languageFolder, "*.config", SearchOption.AllDirectories))
        {
            var source = XElement.Load(file);

            var migratingNotification = new SyncMigratingNotification<Language>(source, context);

            if (_eventAggregator.PublishCancelable(migratingNotification) == true)
            {
                continue;
            }

            var target = MigrateLanguage(source);

            if (target != null)
            {
                var migratedNotification = new SyncMigratedNotification<Language>(target, context).WithStateFrom(migratingNotification);

                _eventAggregator.Publish(migratedNotification);

                messages.Add(SaveTargetXml(migrationId, target));
            }
        }

        return messages;
    }

    private XElement MigrateLanguage(XElement source)
    {
        var alias = source.Attribute("CultureAlias").ValueOrDefault(string.Empty);

        var culture = CultureInfo.GetCultureInfo(alias);
        var key = Int2Guid(culture.LCID);

        var target = new XElement("Language",
            new XAttribute(uSyncConstants.Xml.Key, key),
            new XAttribute(uSyncConstants.Xml.Alias, alias),
            new XAttribute(uSyncConstants.Xml.Level, 0),

            new XElement("IsoCode", alias),
            new XElement("IsMandatory", false),
            new XElement("IsDefault", false));

        return target;
    }

    private MigrationMessage SaveTargetXml(Guid id, XElement xml)
    {
        _migrationFileService.SaveMigrationFile(id, xml, "Languages");

        return new MigrationMessage(ItemType, xml.GetAlias(), MigrationMessageType.Success)
        {
            Message = "When importing migrated languages you may loose your default language value."
        };
    }

    private Guid Int2Guid(int value)
    {
        var bytes = new byte[16];

        BitConverter.GetBytes(value).CopyTo(bytes, 0);

        return new Guid(bytes);
    }

}
