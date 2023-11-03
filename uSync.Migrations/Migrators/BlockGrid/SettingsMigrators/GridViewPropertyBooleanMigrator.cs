namespace uSync.Migrations.Migrators.BlockGrid.SettingsMigrator;

public class GridViewPropertyBooleanMigrator : IGridSettingsViewMigrator
{
    public string ViewKey => "Boolean";

    public string NewDataTypeAlias => "True/false";

    public object ConvertContentString(string value)
    {
        return value;
    }
}