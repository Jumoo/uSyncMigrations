using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using uSync.Migrations.Models;

using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Extensions;

namespace uSync.Migrations.Migrators.DataTypes;
internal class MultipleTextStringMigrator : DataTypeMigratorBase
{
    public override string[] Editors => new[]
    {
        "Umbraco.MultipleTextstring"
    };

    public override object GetConfigValues(SyncDataTypeInfo dataTypeInfo)
    {
        var config = new MultipleTextStringConfiguration();
        return dataTypeInfo.MapPreValues(config);
    }
}
