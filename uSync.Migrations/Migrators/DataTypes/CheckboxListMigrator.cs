using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.DataTypes;
internal class CheckboxListMigrator : DataTypeMigratorBase
{
    public override string[] Editors => new[] { 
        "Umbraco.CheckBoxList",
        "Umbraco.RadioButtonList"
    };

    public override string GetDatabaseType(SyncDataTypeInfo dataTypeInfo)
        => "Nvarchar"; 

    public override object GetConfigValues(SyncDataTypeInfo dataTypeInfo)
    {
        var config = new ValueListConfiguration();

        foreach(var item in dataTypeInfo.PreValues)
        {
            config.Items.Add(new ValueListConfiguration.ValueListItem
            {
                Id = item.SortOrder,
                Value = item.Value
            });
        }

        return config;
    }
}
