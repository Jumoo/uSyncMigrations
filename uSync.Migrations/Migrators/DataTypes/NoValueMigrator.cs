using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.DataTypes;
internal class NoValueMigrator : DataTypeMigratorBase
{
    public override string[] Editors => new[]
    {
        "Umbraco.MemberPicker2",
        "UmbracoForms.FormPicker"
    };

    private Dictionary<string, string> mappings = new Dictionary<string, string>
    {
        { "Umbraco.MemberPicker2", "Umbraco.MemberPicker" }
    };

    public override string GetDataType(SyncDataTypeInfo dataTypeInfo)
    {
        if (mappings.ContainsKey(dataTypeInfo.EditorAlias))
            return mappings[dataTypeInfo.EditorAlias];

        return dataTypeInfo.EditorAlias;
    }
    

    public override object GetConfigValues(SyncDataTypeInfo dataTypeInfo)
        => null;
}
