using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace uSync.Migrations.Migrators.Community.Codery;

[SyncMigrator("Codery.TextCount")]
public class TextCountToTextboxMigrator : SyncPropertyMigratorBase
{
    public override string GetDatabaseType(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => nameof(ValueStorageType.Ntext);

    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.TextBox;

    public override object? GetConfigValues(
        SyncMigrationDataTypeProperty dataTypeProperty,
        SyncMigrationContext context)
    {
        var limitPreValue = dataTypeProperty.PreValues
            .EmptyNull()
            .FirstOrDefault(pv => pv.Alias == "limit");

        int? limitValue = (!string.IsNullOrEmpty(limitPreValue?.Value) && 
                           int.TryParse(limitPreValue.Value, out int parsedValue))
            ? parsedValue
            : null;

        return new TextboxConfiguration
        {
            MaxChars = limitValue
        };
    }
}
