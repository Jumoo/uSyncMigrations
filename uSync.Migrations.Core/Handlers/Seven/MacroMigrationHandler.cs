using System.Xml.Linq;

using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;

using uSync.Core;
using uSync.Migrations.Core.Context;
using uSync.Migrations.Core.Handlers.Shared;
using uSync.Migrations.Core.Services;

namespace uSync.Migrations.Core.Handlers.Seven;

[SyncMigrationHandler(BackOfficeConstants.Groups.Settings, uSyncMigrations.Priorities.Macros,
    SourceVersion = 7,
    SourceFolderName = "Macro",
    TargetFolderName = "Macros")]
internal class MacroMigrationHandler : SharedHandlerBase<Macro>, ISyncMigrationHandler
{
    public MacroMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        ILogger<MacroMigrationHandler> logger)
        : base(eventAggregator, migrationFileService, logger)
    { }

    protected override (string alias, Guid key) GetAliasAndKey(XElement source, SyncMigrationContext? context)
        => (
            alias: source.Element("alias").ValueOrDefault(string.Empty),
            key: source.Element("Key").ValueOrDefault(Guid.Empty)
        );

    protected override XElement? MigrateFile(XElement source, int level, SyncMigrationContext context)
    {
        var (alias, key) = GetAliasAndKey(source, context);

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

    private static string MapPropertyType(string editorAlias)
    {
        if (_mappedTypes.ContainsKey(editorAlias) == true)
        {
            return _mappedTypes[editorAlias];
        }

        return editorAlias;
    }
}
