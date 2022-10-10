using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.DataTypes;
internal class DropdownMigrator : DataTypeMigratorBase
{
    public override string[] Editors => new[]
    {
        "Umbraco.DropDown.Flexible",
        "Umbraco.DropDown",
        "Umbraco.DropDownMultiple"
    };

    public override string GetDataType(SyncDataTypeInfo dataTypeInfo)
        => "Umbraco.DropDown.Flexible";
    
    public override object GetConfigValues(SyncDataTypeInfo dataTypeInfo)
    {
    var config = new DropDownFlexibleConfiguration();

        foreach(var preValue in dataTypeInfo.PreValues)
        {
            if (preValue.Alias.InvariantEquals("multiple"))
            {
                var attempt = preValue.Value.TryConvertTo<bool>();
                if (attempt.Success)
                {
                    config.Multiple = attempt.Result;
                }
            }
            else
            {
                config.Items.Add(new ValueListConfiguration.ValueListItem
                {
                    Id = preValue.SortOrder,
                    Value = preValue.Value
                });
            }
        }

        return config;
    }
}
