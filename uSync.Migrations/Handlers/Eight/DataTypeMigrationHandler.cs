using System.Xml.Linq;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;

using uSync.Core;
using uSync.Migrations.Handlers.Shared;
using uSync.Migrations.Migrators;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Models;
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
        IDataTypeService dataTypeService) : base(eventAggregator, migrationFileService, dataTypeService)
    { }

    protected override string GetDatabaseType(XElement source)
        => source.Element("Info")?.Element("DatabaseType").ValueOrDefault(string.Empty) ?? string.Empty;

    protected override string GetDocTypeFolder(XElement source)
        => source.Element("Info")?.Element("Folder").ValueOrDefault(string.Empty) ?? string.Empty;

    protected override string GetDocTypeName(XElement source)
        => source.Element("Info")?.Element(uSyncConstants.Xml.Name).ValueOrDefault(string.Empty) ?? string.Empty;

    protected override object? GetDataTypeConfig(ISyncPropertyMigrator? migrator, SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => migrator?.GetConfigValues(dataTypeProperty, context);

    protected override SyncMigrationDataTypeProperty GetMigrationDataTypeProperty(string editorAlias, string database, XElement source)
        => new SyncMigrationDataTypeProperty(editorAlias, database, GetXmlConfig(source));

    protected override ReplacementDataTypeInfo? GetReplacementInfo(string editorAlias, string databaseType, XElement source, SyncMigrationContext context)
    {
        // replacements
        //
        var migrator = context.TryGetMigrator(editorAlias);
        if (migrator != null && migrator is ISyncReplacablePropertyMigrator replacablePropertyMigrator)
        {
            return replacablePropertyMigrator.GetReplacementEditorId(
                new SyncMigrationDataTypeProperty(editorAlias, databaseType, GetXmlConfig(source)),
                context);
        }

        return null;
    }

    protected override object? MakeEmptyLabelConfig(SyncMigrationDataTypeProperty dataTypeProperty)
        => dataTypeProperty.ConfigAsString;
    private string? GetXmlConfig(XElement source) 
        => source.Element("Config").ValueOrDefault(string.Empty);
}
