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
    public class uEditorNotesTouEditorNotes : SyncPropertyMigratorBase
    {
        //alias is the same
        //content is the same
        // prevalue config is differently done
        public override object GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        {
            //prevalues map 1-1 between V7 and V10
            var config = new JObject();
            return config.MapPreValues(dataTypeProperty.PreValues, new Dictionary<string, string>
            {
                { "panelTitle", "panelTitle"},
                { "noteCssClass", "noteCssClass" },
                { "editorNotes", "editorNotes" },
                { "hideLabel", "hideLabel" },
                { "noteRenderMode","noteRenderMode" }
            });    

        }
    }
}
