using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.Community
{
    [SyncMigrator("tooorangey.EditorNotes")]
    public class uEditorNotesTouEditorNotesMigrator : SyncPropertyMigratorBase
    {
        //alias is the same
        //content is the same
        // prevalue config is differently done but are 1-1 naming wise
        public override object GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        {
            var config = new JObject();
            return dataTypeProperty.PreValues.ConvertPreValuesToJson(false);    

        }
    }
}
