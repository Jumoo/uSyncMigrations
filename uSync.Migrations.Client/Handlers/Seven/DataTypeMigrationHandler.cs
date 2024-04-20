﻿using System.Xml.Linq;

using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

using uSync.Core;
using uSync.Migrations.Core;
using uSync.Migrations.Core.Context;
using uSync.Migrations.Core.Extensions;
using uSync.Migrations.Core.Handlers;
using uSync.Migrations.Core.Handlers.Shared;
using uSync.Migrations.Core.Migrators;
using uSync.Migrations.Core.Migrators.Models;
using uSync.Migrations.Core.Models;
using uSync.Migrations.Core.Services;
using uSync.Migrations.Core.Validation;


namespace uSync.Migrations.Client.Handlers.Seven;

[SyncMigrationHandler(BackOfficeConstants.Groups.Settings, uSyncMigrations.Priorities.DataTypes,
    SourceVersion = 7,
    SourceFolderName = "DataType",
    TargetFolderName = "DataTypes")]
public class DataTypeMigrationHandler : SharedDataTypeHandler, ISyncMigrationHandler, ISyncMigrationValidator
{
    private readonly SyncPropertyMigratorCollection _migrators;

    public DataTypeMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService fileService,
        IDataTypeService dataTypeService,
        SyncPropertyMigratorCollection migrators,
        ILogger<DataTypeMigrationHandler> logger)
        : base(eventAggregator, fileService, dataTypeService, logger)
    {
        _migrators = migrators;
    }

    protected override (string alias, Guid key) GetAliasAndKey(XElement source, SyncMigrationContext? context)
        => (
            alias: source.Attribute("Name").ValueOrDefault(string.Empty),
            key: source.Attribute("Key").ValueOrDefault(Guid.Empty)
        );

    private (string alias, Guid key) GetAliasAndKey(XElement source, SyncValidationContext context)
    {
        var alias = source.Attribute("Name").ValueOrDefault(string.Empty);
        alias = context.Options.ReplacementAliases?.TryGetValue(alias, out var a) == true ? a : alias;
        return (
            alias,
            key: source.Attribute("Key").ValueOrDefault(Guid.Empty)
        );
    }

    protected override ReplacementDataTypeInfo? GetReplacementInfo(string dataTypeAlias, string editorAlias, string databaseType, XElement source, SyncMigrationContext context)
    {
        //
        // replacements
        //
        if (context.Migrators.TryGetMigrator(editorAlias, out var migrator) is false 
            || migrator is not ISyncReplacablePropertyMigrator replaceablePropertyMigrator)
        {
            return null;
        }

        return replaceablePropertyMigrator.GetReplacementEditorId(
                new SyncMigrationDataTypeProperty(dataTypeAlias, editorAlias, databaseType, GetPreValues(source)),
                context);
    }

    protected override string GetEditorAlias(XElement source)
        => source.Attribute("Id").ValueOrDefault(string.Empty);

    protected override string GetDataTypeName(XElement source)
        => source.Attribute("Name").ValueOrDefault(string.Empty);

    protected override string GetDataTypeFolder(XElement source)
        => source.Attribute("Folder").ValueOrDefault(string.Empty);

    protected override string GetDatabaseType(XElement source)
        => source.Attribute("DatabaseType").ValueOrDefault(string.Empty);

    protected override int GetLevel(XElement source, int level) => level;

    protected override SyncMigrationDataTypeProperty GetMigrationDataTypeProperty(
        string dataTypeAlias, string editorAlias, string database, XElement source)
    {
        var preValues = GetPreValues(source);
        return new SyncMigrationDataTypeProperty(dataTypeAlias, editorAlias, database, preValues);
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


    public IEnumerable<MigrationMessage> Validate(SyncValidationContext validationContext)
    {
        var messages = new List<MigrationMessage>();

        var dataTypes = Path.Combine(validationContext.Options.Source, ItemType);
        var migrators = _migrators.GetPreferredMigratorList(validationContext.Options.PreferredMigrators);

        foreach (var file in Directory.GetFiles(dataTypes, "*.config", SearchOption.AllDirectories))
        {
            try
            {
                var source = XElement.Load(file);
                if (source.IsEmptyItem()) continue;

                var (alias, key) = GetAliasAndKey(source, validationContext);
                var editorAlias = GetEditorAlias(source);

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

    protected override string GetNewEditorAlias(ISyncPropertyMigrator? migrator, SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => migrator?.GetEditorAlias(dataTypeProperty, context) ?? UmbConstants.PropertyEditors.Aliases.Label;

    protected override string GetNewDatabaseType(ISyncPropertyMigrator? migrator, SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => migrator?.GetDatabaseType(dataTypeProperty, context) ?? ValueTypes.String;

    protected override object? GetDataTypeConfig(ISyncPropertyMigrator? migrator, SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        return dataTypeProperty.PreValues != null
            ? migrator?.GetConfigValues(dataTypeProperty, context)
            ?? MakeEmptyLabelConfig(dataTypeProperty)
            : MakeEmptyLabelConfig(dataTypeProperty);
    }
    protected override object? MakeEmptyLabelConfig(SyncMigrationDataTypeProperty dataTypeProperty)
        => dataTypeProperty.PreValues?.ConvertPreValuesToJson(false);
}
