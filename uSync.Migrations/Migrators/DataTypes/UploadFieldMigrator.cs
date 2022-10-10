using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using uSync.Migrations.Models;
using Umbraco.Cms.Core.PropertyEditors;

namespace uSync.Migrations.Migrators.DataTypes;
internal class UploadFieldMigrator : DataTypeMigratorBase
{
    public override string[] Editors => new[] { "Umbraco.UploadField" };


    public override object GetConfigValues(SyncDataTypeInfo dataTypeInfo)
    {
        return new FileUploadConfiguration();
    }
}
