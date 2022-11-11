using System.Xml.Linq;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;
using uSync.Core;
using uSync.Migrations.Composing;
using uSync.Migrations.Migrators;
using uSync.Migrations.Models;
using uSync.Migrations.Notifications;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;

internal class ContentBaseMigrationHandler<TEntity>
    where TEntity : IEntity
{
    private readonly IEventAggregator _eventAggregator;
    private readonly SyncPropertyMigratorCollection _migrators;
    private readonly SyncMigrationFileService _migrationFileService;
    private readonly IShortStringHelper _shortStringHelper;

    protected readonly HashSet<string> _ignoredProperties = new(StringComparer.OrdinalIgnoreCase);

    public ContentBaseMigrationHandler(
        IEventAggregator eventAggregator,
        SyncMigrationFileService migrationFileService,
        SyncPropertyMigratorCollection contentPropertyMigrators,
        IShortStringHelper shortStringHelper)
    {
        _eventAggregator = eventAggregator;
        _migrationFileService = migrationFileService;
        _migrators = contentPropertyMigrators;
        _shortStringHelper = shortStringHelper;
    }

    public IEnumerable<MigrationMessage> DoMigrateFromDisk(Guid id, string folder, SyncMigrationContext context)
    {
        // loads all the content names into the context, so we can get them later on.
        PrepareContext(folder, context);

        var itemType = Path.GetFileName(folder);

        return MigrateFolder(id, itemType, folder, 0, context);
    }

    private IEnumerable<MigrationMessage> MigrateFolder(Guid id, string itemType, string folder, int level, SyncMigrationContext context)
    {
        if (Directory.Exists(folder) == false)
        {
            return Enumerable.Empty<MigrationMessage>();
        }

        var messages = new List<MigrationMessage>();

        foreach (var file in Directory.GetFiles(folder, "*.config"))
        {
            var source = XElement.Load(file);

            var migratingNotification = new SyncMigratingNotification<TEntity>(source, context);

            if (_eventAggregator.PublishCancelable(migratingNotification) == true)
            {
                continue;
            }

            var target = ConvertContent(itemType, source, level, context);

            if (target != null)
            {
                var migratedNotification = new SyncMigratedNotification<TEntity>(target, context).WithStateFrom(migratingNotification);

                _eventAggregator.Publish(migratedNotification);

                messages.Add(SaveTargetXml(itemType, id, target));
            }
        }

        foreach (var childFolder in Directory.GetDirectories(folder))
        {
            messages.AddRange(MigrateFolder(id, itemType, childFolder, level + 1, context));
        }

        return messages;
    }

    private XElement ConvertContent(string itemType, XElement source, int level, SyncMigrationContext context)
    {
        var key = source.Attribute("guid").ValueOrDefault(Guid.Empty);
        var alias = source.Attribute("nodeName").ValueOrDefault(string.Empty);
        var parent = source.Attribute("parentGUID").ValueOrDefault(Guid.Empty);
        var contentType = source.Attribute("nodeTypeAlias").ValueOrDefault(string.Empty);
        var template = source.Attribute("templateAlias").ValueOrDefault(string.Empty);
        var published = source.Attribute("published").ValueOrDefault(false);
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

            if (string.IsNullOrWhiteSpace(template) == false)
            {
                info.Add(new XElement("Template", new XAttribute("Key", context.GetTemplateKey(template)), template));
            }
            else
            {
                info.Add(new XElement("Template"));
            }
        }

        var propertiesList = new XElement("Properties");

        foreach (var property in source.Elements())
        {
            if (_ignoredProperties.Contains(property.Name.LocalName))
            {
                continue;
            }

            var editorAlias = context.GetEditorAlias(contentType, property.Name.LocalName);

            // with the editorAlias, we can do any migrations of the string types here...
            var migratedValue = MigrateContentValue(editorAlias, property.Value, context);

            var newProperty = new XElement(property.Name.LocalName);
            newProperty.Add(new XElement("Value", new XCData(migratedValue)));
            propertiesList.Add(newProperty);
        }

        target.Add(propertiesList);
        return target;
    }

    private MigrationMessage SaveTargetXml(string itemType, Guid id, XElement xml)
    {
        _migrationFileService.SaveMigrationFile(id, xml, xml.Name.LocalName);

        return new MigrationMessage(itemType, xml.GetAlias(), MigrationMessageType.Success);
    }

    private string MigrateContentValue(string editorAlias, string value, SyncMigrationContext context)
    {
        if (_migrators.TryGet(editorAlias, out var migrator) == true)
        {
            return migrator?.GetContentValue(new SyncMigrationContentProperty(editorAlias, value), context) ?? value;
        }

        return value;
    }

    private void PrepareContext(string folder, SyncMigrationContext context)
    {
        var files = Directory.GetFiles(folder, "*.config", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var source = XElement.Load(file);
            var key = source.Attribute("guid").ValueOrDefault(Guid.Empty);
            var alias = source.Attribute("nodeName").ValueOrDefault(string.Empty);

            if (key != Guid.Empty && string.IsNullOrWhiteSpace(alias) == false)
            {
                context.AddContentKey(key, alias);
            }
        }
    }
}
