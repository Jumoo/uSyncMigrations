using System.Xml.Linq;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Umbraco.Cms.Core.Models.ContentEditing;

using uSync.Core;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.DataTypes;
using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;
internal class DataTypeMigrationHandler : ISyncMigrationHandler
{
    private readonly DataTypeMigrationCollection _migrators;
    private readonly MigrationFileService _migrationFileService;
    private ILogger<DataTypeMigrationHandler> _logger;

    private JsonSerializerSettings _jsonSerializerSettings;

    public DataTypeMigrationHandler(
        MigrationFileService fileService,
        ILogger<DataTypeMigrationHandler> logger,
        DataTypeMigrationCollection migrators)
    {
        _migrators = migrators;
        _migrationFileService = fileService;
        _logger = logger;

        _jsonSerializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = new MigrationsContractResolver()
        };
    }


    public int Priority => uSyncMigrations.Priorities.DataTypes;

    public string ItemType => "DataType";

    public IEnumerable<MigrationMessage> MigrateFromDisk(Guid migrationId, string sourceFolder, MigrationContext context)
    {
        return MigrateFolder(migrationId, Path.Combine(sourceFolder, "DataType"), 0, context);
    }

    public void PrepMigrations(Guid migrationId, string sourceFolder, MigrationContext context)
    {
    }

    private IEnumerable<MigrationMessage> MigrateFolder(Guid id, string folder, int level, MigrationContext context)
    {
        if (!Directory.Exists(folder)) return Enumerable.Empty<MigrationMessage>();
        var files = Directory.GetFiles(folder, "*.config").ToList();

        var messages = new List<MigrationMessage>();

        foreach (var file in files)
        {
            var sourceXml = XElement.Load(file);
            var targetXml = MigrateDataType(sourceXml, level, context);
            if (targetXml != null)
            {
                messages.Add(SaveTargetXml(id, targetXml));
            }
        }

        foreach (var childFolder in Directory.GetDirectories(folder))
        {
            messages.AddRange(MigrateFolder(id, childFolder, level + 1, context));
        }

        return messages;
    }

    private MigrationMessage SaveTargetXml(Guid id, XElement xml)
    {
        _migrationFileService.SaveMigrationFile(id, xml, "DataTypes");
        return new MigrationMessage(ItemType, xml.GetAlias(), MigrationMessageType.Success);
    }


    public XElement? MigrateDataType(XElement source, int level, MigrationContext context)
    {
        var key = source.Attribute("Key").ValueOrDefault(string.Empty);
        var editorAlias = source.Attribute("Id").ValueOrDefault(string.Empty);
        var name = source.Attribute("Name").ValueOrDefault(string.Empty);
        var databaseType = source.Attribute("DatabaseType").ValueOrDefault(string.Empty);
        var folder = source.Attribute("Folder").ValueOrDefault(string.Empty);

        // this way we can block certain types of thing (e.g list items)
        if (context.IsBlocked(ItemType, editorAlias)) return null;

        var preValues = GetPreValues(source);

        // change the type of thing as part of a migration.

        var dataTypeInfo = new SyncDataTypeInfo
        {
            EditorAlias = editorAlias,
            DatabaseType = databaseType,
            Name = name,
            PreValues = preValues
        };

        // the migration for this type goes here... 
        var migrator = _migrators.GetMigrator(editorAlias);
        if (migrator == null) _logger.LogWarning("No Migrator for {editorAlias} will make it a label", editorAlias);

        var newEditorAlias = migrator?.GetDataType(dataTypeInfo) ?? "Umbraco.Label";
        var newDatabaseType = migrator?.GetDatabaseType(dataTypeInfo) ?? "STRING";
        var newConfig = dataTypeInfo.PreValues != null
            ? migrator?.GetConfigValues(dataTypeInfo) ?? MakeEmptyLabelConfig(dataTypeInfo)
            : MakeEmptyLabelConfig(dataTypeInfo);


        // now we write the new xml. 
        var target = new XElement("DataType",
            new XAttribute(uSyncConstants.Xml.Key, key),
            new XAttribute(uSyncConstants.Xml.Alias, name),
            new XAttribute(uSyncConstants.Xml.Level, level),
            new XElement("Info",
                new XElement("Name", name),
                new XElement("EditorAlias", newEditorAlias),
                new XElement("DatabaseType", newDatabaseType)));

        if (!string.IsNullOrWhiteSpace(folder))
            target.Element("Info").Add(new XElement("Folder", folder));
        

        if (newConfig != null)
        {
            var config = new XElement("Config", new XCData(
                JsonConvert.SerializeObject(newConfig, Formatting.Indented, _jsonSerializerSettings)));
            target.Add(config);
        }

        return target;
    }

    private IList<PreValue> GetPreValues(XElement source)
    {
        var items = new List<PreValue>();

        var preValues = source.Element("PreValues");
        if (preValues == null) return items;


        foreach (var element in preValues.Elements("PreValue"))
        {
            items.Add(new PreValue
            {
                Alias = element.Attribute("Alias").ValueOrDefault(string.Empty),
                SortOrder = element.Attribute("SortOrder").ValueOrDefault(0),
                Value = element.ValueOrDefault(string.Empty)
            });
        }

        return items;
    }


    private object MakeEmptyLabelConfig(SyncDataTypeInfo dataTypeInfo)
        => dataTypeInfo.ConvertPreValuesToJson(false);

}
