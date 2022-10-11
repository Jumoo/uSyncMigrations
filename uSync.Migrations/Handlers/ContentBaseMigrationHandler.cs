using System.Xml.Linq;

using Umbraco.Cms.Core.Strings;

using Umbraco.Extensions;

using uSync.Core;
using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;

internal class ContentBaseMigrationHandler
{
    private readonly SyncMigratorCollection _migrators;
    private readonly MigrationFileService _migrationFileService;
    private readonly IShortStringHelper _shortStringHelper;

    public virtual string ItemType { get; private set; }

    public ContentBaseMigrationHandler(
        MigrationFileService migrationFileService,
        SyncMigratorCollection contentPropertyMigrators,
        IShortStringHelper shortStringHelper,
        string itemType)
    {
        _migrationFileService = migrationFileService;
        _migrators = contentPropertyMigrators;
        _shortStringHelper = shortStringHelper;
        ItemType = itemType;
    }

    public IEnumerable<MigrationMessage> DoMigrateFromDisk(Guid id, string folder,
    MigrationContext context)
    {
        // loads all the content names into the context, so we can get them later on.
        PrepContext(folder, context);

        var itemType = Path.GetFileName(folder);
        return MigrateFolder(id, itemType, folder, 0, context);
    }

    private IEnumerable<MigrationMessage> MigrateFolder(Guid id, string itemType, string folder, int level, MigrationContext context)
    {
        if (!Directory.Exists(folder)) return Enumerable.Empty<MigrationMessage>();

        var messages = new List<MigrationMessage>();

        foreach (var file in Directory.GetFiles(folder, "*.config"))
        {
            var source = XElement.Load(file);
            var target = ConvertContent(itemType, source, level, context);

            messages.Add(SaveTargetXml(id, target));

        }

        foreach (var childFolder in Directory.GetDirectories(folder))
        {
            messages.AddRange(MigrateFolder(id, itemType, childFolder, level + 1, context));
        }

        return messages;
    }

    private static string[] _ignoredProperties = new[] {
        "umbracoWidth", "umbracoHeight", "umbracoBytes", "umbracoExtension"
    };

    private XElement ConvertContent(string itemType, XElement source, int level, MigrationContext context)
    {
        var key = source.Attribute("guid").ValueOrDefault(Guid.Empty);
        var alias = source.Attribute("nodeName").ValueOrDefault(string.Empty);
        var parent = source.Attribute("parentGUID").ValueOrDefault(Guid.Empty);
        var contentType = source.Attribute("nodeTypeAlias").ValueOrDefault(string.Empty);
        var template = source.Attribute("templateAlias").ValueOrDefault(string.Empty);
        var published = source.Attribute("Published").ValueOrDefault(false);
        var createdDate = source.Attribute("updated").ValueOrDefault(DateTime.Now);
        var sortOrder = source.Attribute("sortOrder").ValueOrDefault(0);

        var path = context.GetContentPath(parent) + "/" + alias.ToSafeAlias(_shortStringHelper);
        context.AddContentPath(key, path);

        var target = new XElement(itemType,

            new XAttribute("Key", key),
            new XAttribute("Alias", alias),
            new XAttribute("Level", level),

            new XElement("Info",
                new XElement("Parent", new XAttribute("Key", parent), context.GetContentAlias(parent)),
                new XElement("Path", path),
                new XElement("Trashed", false),
                new XElement("ContentType", contentType),
                new XElement("CreateDate", createdDate.ToString("s")),
                new XElement("NodeName", new XAttribute("Default", alias)),
                new XElement("SortOrder", sortOrder)));

        if (itemType == "Content")
        {
            var info = target.Element("Info");

            info.Add(new XElement("Published", new XAttribute("Default", published)));
            info.Add(new XElement("Schedule"));
            info.Add(new XElement("Template",
                        new XAttribute("Key", context.GetTemplateKey(template)),
                        template));
        }


        var propertiesList = new XElement("Properties");

        foreach (var property in source.Elements())
        {

            if (_ignoredProperties.InvariantContains(property.Name.LocalName)) continue;

            var editorAlias = context.GetEditorAlias(contentType, property.Name.LocalName);

            // with the editorAlias, we can do any migrations of the string types here...
            var migratedValue = MigrateContentValue(editorAlias, property.Value);

            var newProperty = new XElement(property.Name.LocalName);
            newProperty.Add(new XElement("Value", new XCData(migratedValue)));
            propertiesList.Add(newProperty);
        }

        target.Add(propertiesList);
        return target;
    }

    private MigrationMessage SaveTargetXml(Guid id, XElement xml)
    {
        _migrationFileService.SaveMigrationFile(id, xml, xml.Name.LocalName);
        return new MigrationMessage(this.ItemType, xml.GetAlias(), MigrationMessageType.Success);
    }

    private string MigrateContentValue(string editorAlias, string value)
    {
        var migrator = _migrators.GetMigrator(editorAlias);
        if (migrator != null)
            return migrator.GetContentValue(editorAlias, value);

        // else
        return value;
    }


    private void PrepContext(string folder, MigrationContext context)
    {
        var files = Directory.GetFiles(folder, "*.config", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var source = XElement.Load(file);
            var key = source.Attribute("guid").ValueOrDefault(Guid.Empty);
            var alias = source.Attribute("nodeName").ValueOrDefault(string.Empty);

            if (key != Guid.Empty && !string.IsNullOrEmpty(alias))
                context.AddContentKey(key, alias);

        }
    }

}
