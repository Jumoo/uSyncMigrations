using System.Xml.Linq;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

using uSync.Core;
using uSync.Migrations.Core.Composing;
using uSync.Migrations.Core.Context;
using uSync.Migrations.Core.Handlers.Shared;
using uSync.Migrations.Core.Migrators;
using uSync.Migrations.Core.Migrators.Models;
using uSync.Migrations.Core.Models;
using uSync.Migrations.Core.Services;
using uSync.Migrations.Core.Validation;

namespace uSync.Migrations.Core.Handlers.Eight;
[SyncMigrationHandler(BackOfficeConstants.Groups.Settings, uSyncMigrations.Priorities.DataTypes,
    SourceVersion = 8,
    SourceFolderName = "DataTypes",
    TargetFolderName = "DataTypes")]
internal class DataTypeMigrationHandler : SharedDataTypeHandler, ISyncMigrationHandler, ISyncMigrationValidator
{
    private readonly SyncPropertyMigratorCollection _migrators;

    public DataTypeMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        IDataTypeService dataTypeService,
        ILogger<DataTypeMigrationHandler> logger,
        SyncPropertyMigratorCollection migrators)
        : base(eventAggregator, migrationFileService, dataTypeService, logger)
    {
        _migrators = migrators;
    }

    protected override string GetEditorAlias(XElement source)
        => source.Element("Info")?.Element("EditorAlias").ValueOrDefault(string.Empty) ?? string.Empty;

    string GetName(XElement source)
        => source.Element("Info")?.Element("Name").ValueOrDefault(string.Empty) ?? string.Empty;

    protected override string GetDatabaseType(XElement source)
        => source.Element("Info")?.Element("DatabaseType").ValueOrDefault(string.Empty) ?? string.Empty;

    protected override string GetDataTypeFolder(XElement source)
        => source.Element("Info")?.Element("Folder").ValueOrDefault(string.Empty) ?? string.Empty;

    protected override string GetDataTypeName(XElement source)
        => source.Element("Info")?.Element(uSyncConstants.Xml.Name).ValueOrDefault(string.Empty) ?? string.Empty;

    protected override SyncMigrationDataTypeProperty GetMigrationDataTypeProperty(
        string alias, string editorAlias, string database, XElement source)
        => new SyncMigrationDataTypeProperty(alias, editorAlias, database, GetXmlConfig(source));

    protected override ReplacementDataTypeInfo? GetReplacementInfo(string dataTypeAlias, string editorAlias, string databaseType, XElement source, SyncMigrationContext context)
    {
        // replacements
        //
        var migrator = context.Migrators.TryGetMigrator(editorAlias);
        if (migrator != null && migrator is ISyncReplacablePropertyMigrator replacablePropertyMigrator)
        {
            return replacablePropertyMigrator.GetReplacementEditorId(
                new SyncMigrationDataTypeProperty(dataTypeAlias, editorAlias, databaseType, GetXmlConfig(source)),
                context);
        }

        return null;
    }

    protected override object? MakeEmptyLabelConfig(SyncMigrationDataTypeProperty dataTypeProperty)
        => JToken.Parse(dataTypeProperty.ConfigAsString ?? "");

    // v8 behavior is - if we don't know about it, we leave it alone.
    private string? GetXmlConfig(XElement source)
        => source.Element("Config").ValueOrDefault(string.Empty);

    // v8 behavior is - if we don't know about it, we leave it alone.
    protected override string GetNewEditorAlias(ISyncPropertyMigrator? migrator, SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => migrator?.GetEditorAlias(dataTypeProperty, context) ?? dataTypeProperty.EditorAlias;

    // v8 behavior is - if we don't know about it, we leave it alone.
    protected override string GetNewDatabaseType(ISyncPropertyMigrator? migrator, SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => migrator?.GetDatabaseType(dataTypeProperty, context) ?? dataTypeProperty.DatabaseType;

    // v8 behavior is - if we don't know about it, we leave it alone.
    protected override object? GetDataTypeConfig(ISyncPropertyMigrator? migrator, SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => migrator?.GetConfigValues(dataTypeProperty, context) ?? MakeEmptyLabelConfig(dataTypeProperty);

    public IEnumerable<MigrationMessage> Validate(SyncValidationContext validationContext)
    {
        var messages = new List<MigrationMessage>();

        var dataTypes = Path.Combine(validationContext.Options.Source, "DataTypes");
        var migrators = _migrators.GetPreferredMigratorList(validationContext.Options.PreferredMigrators);

        foreach (var file in Directory.GetFiles(dataTypes, "*.config", SearchOption.AllDirectories))
        {
            try
            {
                var source = XElement.Load(file);
                // don't validate the empties
                if (source.IsEmptyItem()) continue;

                var alias = source.GetAlias();
                var key = source.GetKey();
                var editorAlias = GetEditorAlias(source);

                var name = GetName(source);
                var databaseType = GetDatabaseType(source);

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
