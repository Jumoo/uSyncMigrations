using uSync.Migrations.Migrators.BlockGrid.Models;

namespace uSync.Migrations.Migrators.BlockGrid.SettingsMigrators;

public class GridViewPropertyNumberMigrator : IGridSettingsViewMigrator
{
    public string ViewKey => "Number";

    public string GetNewDataTypeAlias(string gridAlias, string? configItemLabel) => "Numeric";

    public object ConvertContentString(string value)
    {
        return value;
    }

    public NewDataTypeInfo? GetAdditionalDataType(string dataTypeAlias, IEnumerable<GridSettingsConfigurationItemPrevalue>? preValues)
    {
        return null;
    }
}