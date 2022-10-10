using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using uSync.Migrations.Models;

using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Extensions;

namespace uSync.Migrations.Migrators.DataTypes;
internal class MultiNodeTreePickerMigrator : DataTypeMigratorBase
{
    public override string[] Editors => new[]
    {
        "Umbraco.MultiNodeTreePicker"
    };

    public override object GetConfigValues(SyncDataTypeInfo dataTypeInfo)
    {
        var picker = new MultiNodePickerConfiguration();
        return  dataTypeInfo.MapPreValues(picker, new Dictionary<string, string>
        {
            { "filter", nameof(picker.Filter) },
            { "minNumber", nameof(picker.MinNumber) },
            { "maxNumber", nameof(picker.MaxNumber) },
            { "showOpenButton", nameof(picker.ShowOpen) },
            { "startNode", nameof(picker.Filter) }
        });
    }
}
