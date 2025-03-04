﻿using uSync.Migrations.Migrators.BlockGrid.Models;

namespace uSync.Migrations.Migrators.BlockGrid.SettingsMigrators;

public class GridViewPropertyBooleanMigrator : IGridSettingsViewMigrator
{
    public string ViewKey => "Boolean";

    public string GetNewDataTypeAlias(string gridAlias, string? configItemLabel) => "True/false";

    public object ConvertContentString(string value)
    {
        return value;
    }

    public NewDataTypeInfo? GetAdditionalDataType(string dataTypeAlias, IEnumerable<GridSettingsConfigurationItemPrevalue>? preValues)
    {
        return null;
    }
}