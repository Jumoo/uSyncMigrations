using System.Globalization;
using System.Xml.Linq;

using uSync.Core;
using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;
internal class LanguageMigrationHandler : ISyncMigrationHandler
{
    private readonly MigrationFileService _migrationFileService;

    public LanguageMigrationHandler(MigrationFileService migrationFileService)
    {
        _migrationFileService = migrationFileService;
    }

    public string ItemType => "Language";

    public int Priority => uSyncMigrations.Priorities.Languages;

    public IEnumerable<MigrationMessage> MigrateFromDisk(Guid migrationId, string sourceFolder, MigrationContext context)
    {
        var languageFolder = Path.Combine(sourceFolder, "Languages");
        if (!Directory.Exists(languageFolder)) return Enumerable.Empty<MigrationMessage>();

        var messages = new List<MigrationMessage>();

        foreach(var file in Directory.GetFiles(languageFolder, "*.config", SearchOption.AllDirectories))
        {
            var sourceXml = XElement.Load(file);
            var targetXml = MigrateLanguage(sourceXml);

            messages.Add(SaveTargetXml(migrationId, targetXml));
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
            Message = "When importing migrated languages you may loose your default language value"
        };

    }

    private Guid Int2Guid(int value)
    {
        byte[] bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        return new Guid(bytes);
    }


    public void PrepMigrations(Guid migrationId, string sourceFolder, MigrationContext context)
    { }
}
