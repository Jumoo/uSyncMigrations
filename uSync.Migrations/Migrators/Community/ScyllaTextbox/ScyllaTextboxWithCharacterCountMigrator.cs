using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators.Community.ScyllaTextbox;

/// <summary>
/// Migrates the Scylla.TextboxWithCharacterCount property editor to the core Umbraco.Textbox property editor.
/// </summary>
/// <remarks>
/// No changes to the property data required
/// </remarks>
[SyncMigrator("Scylla.TextboxWithCharacterCount")]
public class ScyllaTextboxWithCharacterCountMigrator : SyncPropertyMigratorBase
{
    /// <inheritdoc />
    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
     => UmbConstants.PropertyEditors.Aliases.TextBox;


    /// <inheritdoc />
    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var config = new TextboxConfiguration();

        var mappings = new Dictionary<string, string>
        {
            { "maxCount", nameof(config.MaxChars) }
        };

        return config.MapPreValues(dataTypeProperty.PreValues, mappings);
    }
}