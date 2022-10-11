using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using uSync.Migrations.Models;
using Umbraco.Cms.Core.PropertyEditors;

namespace uSync.Migrations.Migrators;
internal class UploadFieldMigrator : SyncMigratorBase
{
    public override string[] Editors => new[] { "Umbraco.UploadField" };


    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
    {
        return new FileUploadConfiguration();
    }
}
