using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.DataTypes;
internal class NoEdittToLabelMigrator : DataTypeMigratorBase
{
    public override string[] Editors => new[] { "Umbraco.NoEdit" };

    public override string GetDataType(SyncDataTypeInfo dataTypeInfo)
        => "Umbraco.Label";

    public override object GetConfigValues(SyncDataTypeInfo dataTypeInfo)
    {
        var config = new LabelConfiguration();
        config.ValueType = dataTypeInfo.GetPreValueOrDefault("umbracoDataValueType", "STRING");
        return config;
    }
}
