using System.Xml.Linq;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using uSync.Core;
using uSync.Migrations.Composing;
using uSync.Migrations.Migrators;
using uSync.Migrations.Models;
using uSync.Migrations.Notifications;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;

internal abstract class ContentTypeBaseMigrationHandler<TEntity>
    where TEntity : IEntity
{
    private readonly IEventAggregator _eventAggregator;
    private readonly SyncMigrationFileService _migrationFileService;
    private readonly SyncPropertyMigratorCollection _migrators;

    public ContentTypeBaseMigrationHandler(
        IEventAggregator eventAggregator,
        SyncMigrationFileService migrationFileService,
        SyncPropertyMigratorCollection migrators)
    {
        _eventAggregator = eventAggregator;
        _migrationFileService = migrationFileService;
        _migrators = migrators;
    }

    public void PrepareContext(string sourceFolder, SyncMigrationContext context)
    {
        if (Directory.Exists(sourceFolder) == false)
        {
            return;
        }

        foreach (var file in Directory.GetFiles(sourceFolder, "*.config", SearchOption.AllDirectories))
        {
            var source = XElement.Load(file);

            var alias = source.Element("Info")?.Element("Alias")?.ValueOrDefault(string.Empty);

            context.AddContentTypeKey(alias, source.Element("Info")?.Element("Key")?.ValueOrDefault(Guid.Empty));

            var compositions = source.Element("Info")?.Element("Compositions")?.Elements("Composition")?.Select(x => x.Value) ?? Enumerable.Empty<string>();
            context.AddContentTypeCompositions(alias, compositions);

            var properties = source.Element("GenericProperties")?.Elements("GenericProperty") ?? Enumerable.Empty<XElement>();

            foreach (var property in properties)
            {
                context.AddContentProperty(alias,
                    property.Element("Alias")?.ValueOrDefault(string.Empty),
                    property.Element("Type")?.ValueOrDefault(string.Empty));
            }
        }
    }

    public IEnumerable<MigrationMessage> DoMigrateFromDisk(
        Guid id,
        string folder,
        string itemType,
        string targetFolder,
        SyncMigrationContext context)
    {
        return MigrateFolder(id, folder, itemType, targetFolder, 0, context);
    }

    private IEnumerable<MigrationMessage> MigrateFolder(
        Guid id,
        string folder,
        string itemType,
        string targetFolder,
        int level,
        SyncMigrationContext context)
    {
        if (Directory.Exists(folder) == false)
        {
            return Enumerable.Empty<MigrationMessage>();
        }

        var messages = new List<MigrationMessage>();

        var files = Directory.GetFiles(folder, "*.config").ToList();

        foreach (var file in files)
        {
            var source = XElement.Load(file);

            var migratingNotification = new SyncMigratingNotification<TEntity>(source, context);

            if (_eventAggregator.PublishCancelable(migratingNotification) == true)
            {
                continue;
            }

            var target = ConvertContentType(source, itemType, level, context);

            if (target != null)
            {
                var migratedNotification = new SyncMigratedNotification<TEntity>(target, context).WithStateFrom(migratingNotification);

                _eventAggregator.Publish(migratedNotification);

                messages.Add(SaveTargetXml(itemType, id, migratedNotification.Xml, targetFolder));
            }
        }

        foreach (var childFolder in Directory.GetDirectories(folder))
        {
            messages.AddRange(MigrateFolder(id, childFolder, itemType, targetFolder, level + 1, context));
        }

        return messages;
    }

    private MigrationMessage SaveTargetXml(string itemType, Guid id, XElement xml, string folder)
    {
        _migrationFileService.SaveMigrationFile(id, xml, folder);

        return new MigrationMessage(itemType, xml.GetAlias(), MigrationMessageType.Success);
    }

    private XElement? ConvertContentType(
        XElement source,
        string itemType,
        int level,
        SyncMigrationContext context)
    {
        var info = source.Element("Info");
        var key = info.Element("Key").ValueOrDefault(Guid.Empty);
        var alias = info.Element("Alias").ValueOrDefault(string.Empty);

        if (context.IsBlocked(itemType, alias)) return null;

        var target = new XElement(itemType,
            new XAttribute(uSyncConstants.Xml.Key, key),
            new XAttribute(uSyncConstants.Xml.Alias, alias),
            new XAttribute(uSyncConstants.Xml.Level, level));

        // update info element. 
        UpdateInfo(info, target);

        // structure
        UpdateStructure(source, target);

        // properties. 
        UpdateProperties(source, target, alias, context);

        // tabs
        UpdateTabs(source, target);

        return target;
    }

    private static void UpdateTabs(XElement source, XElement target)
    {
        var tabs = source.Element("Tabs");
        if (tabs != null)
        {
            var newTabs = new XElement("Tabs");
            foreach (var tab in tabs.Elements("Tab"))
            {
                var newTab = XElement.Parse(tab.ToString());
                newTab.Add(new XElement("Alias", tab.Element("Caption").ValueOrDefault(string.Empty)));
                newTab.Add(new XElement("Type", "Group"));
                newTabs.Add(newTab);
            }
            target.Add(newTabs);
        }
    }

    private void UpdateProperties(
        XElement source,
        XElement target,
        string contentTypeAlias,
        SyncMigrationContext context)
    {
        var properties = source.Element("GenericProperties");

        var newProperties = new XElement("GenericProperties");
        if (properties != null)
        {
            foreach (var property in properties.Elements("GenericProperty"))
            {
                var newProperty = XElement.Parse(property.ToString());

                UpdatePropertyEditor(newProperty, context);

                newProperty.Add(new XElement("Variations", "Nothing"));
                newProperty.Add(new XElement("MandatoryMessage", string.Empty));
                newProperty.Add(new XElement("ValidationRegExpMessage", string.Empty));
                newProperty.Add(new XElement("LabelOnTop", false));

                var tabNode = newProperty.Element("Tab");
                if (tabNode != null)
                {
                    tabNode.Add(new XAttribute("Alias", tabNode.ValueOrDefault(string.Empty)));
                }

                newProperties.Add(newProperty);
            }
        }

        target.Add(newProperties);
    }

    /// <summary>
    ///  Get the editor Alias for this property (it might have updated)
    /// </summary>
    /// <param name="newProperty"></param>
    private void UpdatePropertyEditor(XElement newProperty, SyncMigrationContext context)
    {
        var editorAlias = newProperty.Element("Type").ValueOrDefault(string.Empty);


        // TODO: [KJ] This is best way - and will fail when we get to more advanced editors like vorto. 
        if (string.IsNullOrEmpty(editorAlias) == false &&
            _migrators.TryGet(editorAlias, out var migrator) == true &&
            migrator != null)
        {
            newProperty.Element("Type").Value = migrator.GetEditorAlias(
                new SyncMigrationDataTypeProperty(editorAlias, string.Empty, new List<PreValue>()), context);
        }
    }

    private void UpdateInfo(XElement? info, XElement target)
    {
        var targetInfo = XElement.Parse(info.ToString());
        targetInfo.Element("Key")?.Remove();
        targetInfo.Element("Alias")?.Remove();

        targetInfo.Add(new XElement("Variations", "Nothing"));
        targetInfo.Add(new XElement("IsElement", false));

        target.Add(targetInfo);
    }

    private static void UpdateStructure(XElement source, XElement target)
    {
        var sourceStructure = source.Element("Structure");
        if (sourceStructure != null)
            target.Add(XElement.Parse(sourceStructure.ToString()));
    }
}
