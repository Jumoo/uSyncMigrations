using System.Xml.Linq;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;

using uSync.Core;
using uSync.Migrations.Context;
using uSync.Migrations.Handlers.Shared;
using uSync.Migrations.Migrators;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Eight;
[SyncMigrationHandler(BackOfficeConstants.Groups.Settings, uSyncMigrations.Priorities.DataTypes, 
    SourceVersion = 8,
    SourceFolderName = "DataTypes",
    TargetFolderName = "DataTypes")]
internal class DataTypeMigrationHandler : SharedDataTypeHandler, ISyncMigrationHandler
{
    public DataTypeMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        IDataTypeService dataTypeService,
        ILogger<DataTypeMigrationHandler> logger) 
        : base(eventAggregator, migrationFileService, dataTypeService, logger)
    { }

    protected override string GetEditorAlias(XElement source)
        => source.Element("Info")?.Element("EditorAlias").ValueOrDefault(string.Empty) ?? string.Empty;

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

}
