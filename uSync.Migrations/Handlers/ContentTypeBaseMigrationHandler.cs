using System.Xml.Linq;

using uSync.Core;
using uSync.Migrations.Migrators.DataTypes;
using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;
internal class ContentTypeBaseMigrationHandler
{
    public virtual string ItemType { get; protected set; }

    private readonly MigrationFileService _migrationFileService;
    private readonly DataTypeMigrationCollection _dataTypeMigrators;

    public ContentTypeBaseMigrationHandler(
        MigrationFileService migrationFileService,
        DataTypeMigrationCollection dataTypeMigrators)
    {
        _migrationFileService = migrationFileService;
        _dataTypeMigrators = dataTypeMigrators;
    }

    public void PrepContext(string sourceFolder, MigrationContext context)
    {
        if (!Directory.Exists(sourceFolder)) return;

        foreach(var file in Directory.GetFiles(sourceFolder, "*.config", SearchOption.AllDirectories))
        {

            var source = XElement.Load(file);

            var alias = source.Element("Info").Element("Alias").ValueOrDefault(string.Empty);
            context.AddContentTypeKey(
                alias, source.Element("Info").Element("Key").ValueOrDefault(Guid.Empty));

            var properties = source.Element("GenericProperties")?.Elements("GenericProperty") ?? Enumerable.Empty<XElement>();

            foreach(var property in properties)
            {
                context.AddContentProperty(alias,
                              property.Element("Alias").ValueOrDefault(string.Empty),
                              property.Element("Type").ValueOrDefault(string.Empty));
            }
        }
    }

    public IEnumerable<MigrationMessage> MigrateFromDisk(Guid id, string folder, string baseType, string targetFolder, MigrationContext context)
    {
        return MigrateFolder(id, folder, baseType, targetFolder, 0, context);
    }

    private IEnumerable<MigrationMessage> MigrateFolder(Guid id, string folder, string baseType,
        string targetFolder, int level, MigrationContext context)
    {
        if (!Directory.Exists(folder)) return Enumerable.Empty<MigrationMessage>();

        var messages = new List<MigrationMessage>();

        var files = Directory.GetFiles(folder, "*.config").ToList();

        foreach (var file in files)
        {
            var source = XElement.Load(file);

            var target = ConvertContentType(source, baseType, level, context);
            if (target != null)
            {
                messages.Add(SaveTargetXml(id, target, targetFolder));
            }
        
        }

        foreach (var childFolder in Directory.GetDirectories(folder))
        {
            messages.AddRange(MigrateFolder(id, childFolder, baseType, targetFolder, level + 1, context));
        }

        return messages;
    }
    
    private MigrationMessage SaveTargetXml(Guid id, XElement xml, string folder)
    {
        _migrationFileService.SaveMigrationFile(id, xml, folder);
        return new MigrationMessage(ItemType, xml.GetAlias(), MigrationMessageType.Success);
    }

    private XElement? ConvertContentType(XElement source, string baseType, int level, MigrationContext context)
    {
        var info = source.Element("Info");
        var key = info.Element("Key").ValueOrDefault(Guid.Empty);
        var alias = info.Element("Alias").ValueOrDefault(string.Empty);

        if (context.IsBlocked(ItemType, alias)) return null;

        var target = new XElement(baseType,
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

    private void UpdateProperties(XElement source, XElement target,
        string contentTypeAlias,
        MigrationContext context)
    {
        var properties = source.Element("GenericProperties");

        var newProperties = new XElement("GenericProperties");
        if (properties != null)
        {
            foreach (var property in properties.Elements("GenericProperty"))
            {
                var newProperty = XElement.Parse(property.ToString());

                UpdatePropertyEditor(newProperty);

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

    private void UpdatePropertyEditor(XElement newProperty)
    {
        var type = newProperty.Element("Type").ValueOrDefault(string.Empty);
        if (!string.IsNullOrEmpty(type))
        {
            var migrator = _dataTypeMigrators.GetMigrator(type);
            if (migrator != null)
            {
                newProperty.Element("Type").Value = migrator.GetDataType(new SyncDataTypeInfo
                {
                    EditorAlias = type,
                });
            }
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
