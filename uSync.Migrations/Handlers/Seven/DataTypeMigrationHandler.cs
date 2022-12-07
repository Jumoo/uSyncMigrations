using System.Xml.Linq;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

using uSync.Core;
using uSync.Migrations.Composing;
using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Extensions;
using uSync.Migrations.Handlers.Shared;
using uSync.Migrations.Migrators;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Models;
using uSync.Migrations.Services;
using uSync.Migrations.Validation;

namespace uSync.Migrations.Handlers.Seven;

[SyncMigrationHandler(BackOfficeConstants.Groups.Settings, uSyncMigrations.Priorities.DataTypes,
    SourceVersion = 7,
    SourceFolderName = "DataType",
    TargetFolderName = "DataTypes")]
internal class DataTypeMigrationHandler : SharedDataTypeHandler, ISyncMigrationHandler, ISyncMigrationValidator
{
    private readonly SyncPropertyMigratorCollection _migrators;

    public DataTypeMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService fileService,
        IDataTypeService dataTypeService,
        SyncPropertyMigratorCollection migrators)
        : base(eventAggregator, fileService, dataTypeService)
    {
        _migrators = migrators;
    }

    protected override (string alias, Guid key) GetAliasAndKey(XElement source)
        => (
            alias: source.Attribute("Id").ValueOrDefault(string.Empty),
            key: source.Attribute("Key").ValueOrDefault(Guid.Empty)
        );

    protected override ReplacementDataTypeInfo? GetReplacementInfo(string editorAlias, string databaseType, XElement source, SyncMigrationContext context)
    {
        //
        // replacements
        //
        var migrator = context.TryGetMigrator(editorAlias);
        if (migrator != null && migrator is ISyncReplacablePropertyMigrator replacablePropertyMigrator)
        {
            return replacablePropertyMigrator.GetReplacementEditorId(
                new SyncMigrationDataTypeProperty(editorAlias, databaseType, GetPreValues(source)),
                context);
        }

        return null;
    }

    protected override string GetDocTypeName(XElement source)
        => source.Attribute("Name").ValueOrDefault(string.Empty);

    protected override string GetDocTypeFolder(XElement source)
        => source.Attribute("Folder").ValueOrDefault(string.Empty);

    protected override string GetDatabaseType(XElement source)
        => source.Attribute("DatabaseType").ValueOrDefault(string.Empty);

    protected override SyncMigrationDataTypeProperty GetMigrationDataTypeProperty(string editorAlias, string database, XElement source)
    {
        var preValues = GetPreValues(source);
        return new SyncMigrationDataTypeProperty(editorAlias, database, preValues);
    }

    protected override object? GetDataTypeConfig(ISyncPropertyMigrator? migrator, SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        return dataTypeProperty.PreValues != null
            ? migrator?.GetConfigValues(dataTypeProperty, context)
            ?? MakeEmptyLabelConfig(dataTypeProperty)
            : MakeEmptyLabelConfig(dataTypeProperty);
    }
    protected override object? MakeEmptyLabelConfig(SyncMigrationDataTypeProperty dataTypeProperty)
        => dataTypeProperty.PreValues?.ConvertPreValuesToJson(false);

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
