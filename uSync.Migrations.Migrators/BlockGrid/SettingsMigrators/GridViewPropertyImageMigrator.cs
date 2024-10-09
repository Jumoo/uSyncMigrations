using uSync.Migrations.Migrators.BlockGrid.Models;

namespace uSync.Migrations.Migrators.BlockGrid.SettingsMigrators;

public class GridViewPropertyImageMigrator : IGridSettingsViewMigrator
{
    public string ViewKey => "ImagePicker";

    public string GetNewDataTypeAlias(string gridAlias, string? configItemLabel) => "Image Media Picker";

    public object ConvertContentString(string value)
    {
        return value;
    }

    public NewDataTypeInfo? GetAdditionalDataType(string dataTypeAlias, IEnumerable<GridSettingsConfigurationItemPrevalue>? preValues)
    {
        return null;
    }
}