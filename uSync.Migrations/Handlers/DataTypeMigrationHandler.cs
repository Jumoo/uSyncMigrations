using System.Xml.Linq;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

using uSync.Core;
using uSync.Migrations.Composing;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Models;
using uSync.Migrations.Notifications;
using uSync.Migrations.Serialization;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;

internal class DataTypeMigrationHandler : ISyncMigrationHandler
{
    private readonly IEventAggregator _eventAggregator;
    private readonly SyncPropertyMigratorCollection _migrators;
    private readonly SyncMigrationFileService _migrationFileService;
    private readonly ILogger<DataTypeMigrationHandler> _logger;
    private readonly JsonSerializerSettings _jsonSerializerSettings;
    private readonly IDataTypeService _dataTypeService;

    public DataTypeMigrationHandler(
        IEventAggregator eventAggregator,
        SyncMigrationFileService fileService,
        ILogger<DataTypeMigrationHandler> logger,
        SyncPropertyMigratorCollection migrators,
        IDataTypeService dataTypeService)
    {
        _eventAggregator = eventAggregator;
        _migrators = migrators;
        _migrationFileService = fileService;
        _logger = logger;

        _jsonSerializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = new SyncMigrationsContractResolver(),
            Formatting = Formatting.Indented,
        };
        _dataTypeService = dataTypeService;
    }

    public string Group => uSync.BackOffice.uSyncConstants.Groups.Settings;

    public string ItemType => nameof(DataType);

    public int Priority => uSyncMigrations.Priorities.DataTypes;

    public void PrepareMigrations(Guid migrationId, string sourceFolder, SyncMigrationContext context)
    {
        if (Directory.Exists(sourceFolder) == false)
        {
            return;
        }

        foreach (var file in Directory.GetFiles(sourceFolder, "*.config", SearchOption.AllDirectories))
        {
            var source = XElement.Load(file);
            var dtd = source.Attribute("Key").ValueOrDefault(Guid.Empty);
            var databaseType = source.Attribute("DatabaseType").ValueOrDefault(string.Empty);
            var editorAlias = source.Attribute("Id").ValueOrDefault(string.Empty);
            if (dtd == Guid.Empty || string.IsNullOrEmpty(editorAlias)) continue;

            //
            // replacements
            //
            if (_migrators.TryGet(editorAlias, out ISyncPropertyMigrator migrator)
                && (migrator is not null)
                && (migrator is ISyncReplacablePropertyMigrator replacablePropertyMigrator))
            {
                var replacementInfo = replacablePropertyMigrator.GetReplacementEditorId(
                    new SyncMigrationDataTypeProperty(editorAlias, databaseType, GetPreValues(source)),
                    context);

                if (replacementInfo != null)
                {
                    context.AddReplacementDataType(dtd, replacementInfo.Key);
                    context.AddDataTypeDefinition(dtd, replacementInfo.EditorAlias);

                    if (string.IsNullOrWhiteSpace(replacementInfo.Variation) == false)
                    {
                        context.AddDataTypeVariation(dtd, replacementInfo.Variation);
                    }
                }
            }

            // add alias, (won't update if replacement was added)
            context.AddDataTypeDefinition(dtd, editorAlias);

        }

        foreach (var datatype in _dataTypeService.GetAll())
        {
            context.AddDataTypeDefinition(datatype.Key, datatype.EditorAlias);
        }

    }

    public IEnumerable<MigrationMessage> MigrateFromDisk(Guid migrationId, string sourceFolder, SyncMigrationContext context)
    {
        return MigrateFolder(migrationId, Path.Combine(sourceFolder, ItemType), 0, context);
    }

    private IEnumerable<MigrationMessage> MigrateFolder(Guid id, string folder, int level, SyncMigrationContext context)
    {
        if (Directory.Exists(folder) == false)
        {
            return Enumerable.Empty<MigrationMessage>();
        }

        var files = Directory.GetFiles(folder, "*.config").ToList();

        var messages = new List<MigrationMessage>();

        foreach (var file in files)
        {
            var source = XElement.Load(file);

            var migratingNotification = new SyncMigratingNotification<DataType>(source, context);

            if (_eventAggregator.PublishCancelable(migratingNotification) == true)
            {
                continue;
            }

            var target = MigrateDataType(source, level, context);

            if (target != null)
            {
                var migratedNotification = new SyncMigratedNotification<DataType>(target, context).WithStateFrom(migratingNotification);

                _eventAggregator.Publish(migratedNotification);

                messages.Add(SaveTargetXml(id, target));
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

    public XElement? MigrateDataType(XElement source, int level, SyncMigrationContext context)
    {
        var key = source.Attribute("Key").ValueOrDefault(Guid.Empty);
        var editorAlias = source.Attribute("Id").ValueOrDefault(string.Empty);
        var name = source.Attribute("Name").ValueOrDefault(string.Empty);
        var databaseType = source.Attribute("DatabaseType").ValueOrDefault(string.Empty);
        var folder = source.Attribute("Folder").ValueOrDefault(string.Empty);

        // this way we can block certain types of thing (e.g list items)
        if (context.IsBlocked(ItemType, editorAlias) == true)
        {
            return null;
        }

        if (context.GetReplacementDataType(key) != key)
        {
            // this datatype has been replaced 
            return null;
        }

        var preValues = GetPreValues(source);

        // change the type of thing as part of a migration.

        // the migration for this type goes here...
        var hasMigrator = _migrators.TryGet(editorAlias, out var migrator);
        if (hasMigrator == false)
        {
            _logger.LogWarning("No migrator for {editorAlias} will make it a label.", editorAlias);
        }

        var dataTypeProperty = new SyncMigrationDataTypeProperty(editorAlias, databaseType, preValues);

        var newEditorAlias = migrator?.GetEditorAlias(dataTypeProperty, context) ?? UmbConstants.PropertyEditors.Aliases.Label;
        var newDatabaseType = migrator?.GetDatabaseType(dataTypeProperty, context) ?? ValueTypes.String;

        var newConfig = preValues != null
            ? migrator?.GetConfigValues(dataTypeProperty, context) ?? MakeEmptyLabelConfig(preValues)
            : MakeEmptyLabelConfig(preValues);

        // now we write the new xml. 
        var target = new XElement("DataType",
            new XAttribute(uSyncConstants.Xml.Key, key),
            new XAttribute(uSyncConstants.Xml.Alias, name),
            new XAttribute(uSyncConstants.Xml.Level, level),
            new XElement(uSyncConstants.Xml.Info,
                new XElement(uSyncConstants.Xml.Name, name),
                new XElement("EditorAlias", newEditorAlias),
                new XElement("DatabaseType", newDatabaseType)));

        if (string.IsNullOrWhiteSpace(folder) == false)
        {
            target.Element(uSyncConstants.Xml.Info)?.Add(new XElement("Folder", folder));
        }

        if (newConfig != null)
        {
            target.Add(new XElement("Config",
                new XCData(JsonConvert.SerializeObject(newConfig, _jsonSerializerSettings))));
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

    private object? MakeEmptyLabelConfig(IList<PreValue>? preValues)
        => preValues?.ConvertPreValuesToJson(false);
}
