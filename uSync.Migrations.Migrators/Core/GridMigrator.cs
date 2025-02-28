using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

using uSync.Migrations.Core;
using uSync.Migrations.Core.Legacy.Grid;

namespace uSync.Migrations.Migrators.Core;

/// <summary>
///  Grid Migrator - (migrating grid to grid)
/// </summary>
/// <remarks>
///  in all likelihood you will want to turn grids into block grids or lists.
///  
///  For that there we have the blockGrid migrator, but this migrator is just 
///  for the straight migration path.
///  
///  the main feature here is migrating DTGE elements between grids, 
///  
/// </remarks>
[SyncMigrator(uSyncMigrations.EditorAliases.Grid, typeof(LegacyGridConfiguration), IsDefaultAlias = true)]
[SyncDefaultMigrator]

public class GridMigrator : SyncPropertyMigratorBase
{
    private static string _dtgeContentTypeAliasValue = "dtgeContentTypeAlias";

    private readonly ILogger<GridMigrator> _logger;


    public GridMigrator(ILogger<GridMigrator> logger)
    {
        _logger = logger;
    }

    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => uSyncMigrations.EditorAliases.Grid;

    public override string GetDatabaseType(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => nameof(ValueStorageType.Ntext);

    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var ignoreUserStartNodes = dataTypeProperty.PreValues?.SingleOrDefault(x => x.Alias == "ignoreUserStartNodes");
        if (ignoreUserStartNodes != null)
        {
            bool newValue = bool.TryParse(ignoreUserStartNodes.Value, out var parsedValue)
                ? parsedValue
                : false;

            ignoreUserStartNodes.Value = newValue.ToString();
        }

        return base.GetConfigValues(dataTypeProperty, context);
    }

    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (contentProperty.Value == null) return string.Empty;
        var grid = JsonConvert.DeserializeObject<LegacyGridValue>(contentProperty.Value);
        if (grid == null) return contentProperty.Value;


        foreach (var control in grid.Sections
            .SelectMany(s => s.Rows)
            .SelectMany(r => r.Areas)
            .SelectMany(a => a.Controls))
        {
            if (control?.Value == null || !control.Value.HasValues) continue;

            if (isDoctypeGridEditorControl(control))
            {
                var updatedValues = GetPropertyValues(control, context);
                var updatedValuesSerialized = JsonConvert.SerializeObject(updatedValues);

                control.Value["value"] =
                    JsonConvert.DeserializeObject<JToken>(updatedValuesSerialized);
            }
        }

        return JsonConvert.SerializeObject(grid);
    }

    private string GetDTGEContentTypeAlias(LegacyGridValue.LegacyGridControl control)
    => control.Value?.Value<string>(_dtgeContentTypeAliasValue) ?? string.Empty;

    private bool isDoctypeGridEditorControl(LegacyGridValue.LegacyGridControl control)
        => !string.IsNullOrEmpty(control.Value?.Value<string>(_dtgeContentTypeAliasValue));

    private Dictionary<string, object?> GetPropertyValues(
        LegacyGridValue.LegacyGridControl control, SyncMigrationContext context)
    {
        var propertyValues = new Dictionary<string, object?>();

        var contentTypeAlias = GetDTGEContentTypeAlias(control);
        if (string.IsNullOrWhiteSpace(contentTypeAlias)) return propertyValues;

        var elementValue = control.Value?.Value<JObject>("value")?
               .ToObject<IDictionary<string, object>>();
        if (elementValue == null) return propertyValues;

        foreach (var (propertyAlias, value) in elementValue)
        {
            if (context.ContentTypes.TryGetEditorAliasByTypeAndProperty(contentTypeAlias, propertyAlias, out var editorAliasInfo) is false) { continue; }
            if (context.Migrators.TryGetMigrator(editorAliasInfo.OriginalEditorAlias, out var migrator) is false) { continue; }

            var valueToConvert = (value?.ToString() ?? "").Trim();

            var property = new SyncMigrationContentProperty(
                editorAliasInfo.OriginalEditorAlias,
                propertyAlias,
                editorAliasInfo.OriginalEditorAlias,
                valueToConvert);

            var convertedValue = migrator.GetContentValue(property, context);

            object? propertyValue;
            if (convertedValue?.Trim().DetectIsJson() == true)
            {
                propertyValue = JsonConvert.DeserializeObject(convertedValue ?? "");
            }
            else
            {
                propertyValue = convertedValue;
            }


            propertyValues[propertyAlias] = propertyValue;
        }

        return propertyValues;
    }

}