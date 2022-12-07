using System.Xml.Linq;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

using uSync.Core;
using uSync.Migrations.Composing;
using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Models;
using uSync.Migrations.Serialization;
using uSync.Migrations.Services;
using uSync.Migrations.Validation;

namespace uSync.Migrations.Handlers.Seven;

[SyncMigrationHandler(BackOfficeConstants.Groups.Settings, uSyncMigrations.Priorities.DataTypes, 7,
    SourceFolderName = "DataType",
    TargetFolderName = "DataTypes")]
internal class DataTypeMigrationHandler : MigrationHandlerBase<DataType>, ISyncMigrationHandler, ISyncMigrationValidator
{
    private readonly ILogger<DataTypeMigrationHandler> _logger;
    private readonly JsonSerializerSettings _jsonSerializerSettings;
    private readonly IDataTypeService _dataTypeService;
    private readonly SyncPropertyMigratorCollection _migrators;

    public DataTypeMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService fileService,
        ILogger<DataTypeMigrationHandler> logger,
        IDataTypeService dataTypeService,
        SyncPropertyMigratorCollection migrators)
        : base(eventAggregator, fileService)
    {
        _logger = logger;

        _jsonSerializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = new SyncMigrationsContractResolver(),
            Formatting = Formatting.Indented,
        };
        _dataTypeService = dataTypeService;
        _migrators = migrators;
    }

    public override void Prepare(SyncMigrationContext context)
    {
        foreach (var datatype in _dataTypeService.GetAll())
        {
            context.AddDataTypeDefinition(datatype.Key, datatype.EditorAlias);
        }
    }

    protected override (string alias, Guid key) GetAliasAndKey(XElement source)
        => (
            alias: source.Attribute("Id").ValueOrDefault(string.Empty),
            key: source.Attribute("Key").ValueOrDefault(Guid.Empty)
        );

    protected override void PrepareFile(XElement source, SyncMigrationContext context)
    {
        var (editorAlias, dtd) = GetAliasAndKey(source);
        if (dtd == Guid.Empty || string.IsNullOrEmpty(editorAlias)) return;

        var databaseType = source.Attribute("DatabaseType").ValueOrDefault(string.Empty);

        //
        // replacements
        //
        var migrator = context.TryGetMigrator(editorAlias);
        if (migrator != null && migrator is ISyncReplacablePropertyMigrator replacablePropertyMigrator)
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


    protected override XElement? MigrateFile(XElement source, int level, SyncMigrationContext context)
    {
        var (editorAlias, key) = GetAliasAndKey(source);

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
        var migrator = context.TryGetMigrator(editorAlias);
        if (migrator is null)
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

    private static IList<PreValue> GetPreValues(XElement source)
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

    private static object? MakeEmptyLabelConfig(IList<PreValue>? preValues)
        => preValues?.ConvertPreValuesToJson(false);

    public IEnumerable<MigrationMessage> Validate(MigrationOptions options)
    {
        var messages = new List<MigrationMessage>();

        var dataTypes = Path.Combine(options.Source, ItemType);
        var migrators = _migrators.GetPreferredMigratorList(options.PreferredMigrators);

        foreach (var file in Directory.GetFiles(dataTypes, "*.config", SearchOption.AllDirectories))
        {
            try
            {
                var source = XElement.Load(file);
                var (editorAlias, key) = GetAliasAndKey(source);

                var name = source.Attribute("Name").ValueOrDefault(string.Empty);
                var databaseType = source.Attribute("DatabaseType").ValueOrDefault(string.Empty);

                if (key == Guid.Empty) throw new Exception("Missing Key value");
                if (string.IsNullOrEmpty(editorAlias)) throw new Exception("Id (EditorAlias) value");
                if (string.IsNullOrEmpty(name)) throw new Exception("Missing Name value");
                if (string.IsNullOrEmpty(databaseType)) throw new Exception("Missing database type");

                if (!migrators.Any(x => x.EditorAlias.InvariantEquals(editorAlias)))
                {
                    messages.Add(new MigrationMessage(ItemType, name, MigrationMessageType.Warning)
                    {
                        Message = $"there is no migrator for {editorAlias} value will be untouched but might not import correctly"
                    });
                }
            }
            catch (Exception ex)
            {
                messages.Add(new MigrationMessage(ItemType, Path.GetFileName(file), MigrationMessageType.Error)
                {
                    Message = $"The Datatype file seems to be corrupt or missing something {ex.Message}"
                });
            }
        }

        return messages;
    }
}
