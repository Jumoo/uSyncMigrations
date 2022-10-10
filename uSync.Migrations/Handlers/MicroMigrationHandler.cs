using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using uSync.Core;
using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;
internal class MicroMigrationHandler : ISyncMigrationHandler
{
    private readonly MigrationFileService _migrationFileService;

    public MicroMigrationHandler(MigrationFileService migrationFileService)
    {
        _migrationFileService = migrationFileService;
    }

    public string ItemType => "Macro";

    public int Priority => uSyncMigrations.Priorities.Macros;

    public IEnumerable<MigrationMessage> MigrateFromDisk(Guid migrationId, string sourceFolder, MigrationContext context)
    {

        var macroFolder = Path.Combine(sourceFolder, "macro");
        if (!Directory.Exists(macroFolder)) return Enumerable.Empty<MigrationMessage>();

        var messages = new List<MigrationMessage>();

        foreach(var file in Directory.GetFiles(macroFolder, "*.config", SearchOption.AllDirectories))
        {
            var sourceXml = XElement.Load(file);
            var targetXml = MigrateMacro(sourceXml);

            messages.Add(SaveTargetXml(migrationId, targetXml));
        }

        return messages;
    }

    private XElement MigrateMacro(XElement source) 
    {
        var key = source.Element("Key").ValueOrDefault(Guid.Empty);
        var alias = source.Element("alias").ValueOrDefault(String.Empty);

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

    private static Dictionary<string, string> _mappedTypes = new Dictionary<string, string>
    {
        { "Umbraco.ContentPicker2", "Umbraco.ContentPicker" },
        { "Umbraco.MediaPicker2", "Umbraco.MediaPicker" },
        { "Umbraco.ContentPickerAlias", "Umbraco.ContentPicker" }
    };

    private string MapPropertyType(string editorAlias)
    {
        if (_mappedTypes.ContainsKey(editorAlias))
            return _mappedTypes[editorAlias];

        return editorAlias;
    }

    private MigrationMessage SaveTargetXml(Guid id, XElement xml)
    {
        _migrationFileService.SaveMigrationFile(id, xml, "Macros");
        return new MigrationMessage(ItemType, xml.GetAlias(), MigrationMessageType.Success);

    }


    public void PrepMigrations(Guid migrationId, string sourceFolder, MigrationContext context)
    { }
}
