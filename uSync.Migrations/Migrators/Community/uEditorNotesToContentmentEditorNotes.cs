using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.Community
{
    [SyncMigrator("tooorangey.EditorNotes")]
    public class uEditorNotesToContentmentEditorNotes : SyncPropertyMigratorBase
    {
        public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
      => "Umbraco.Community.Contentment.EditorNotes";
        public override object GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        {
            var config = new JObject();
            config = dataTypeProperty.PreValues.ConvertPreValuesToJson(false,new Dictionary<string, string>
            {
                { "panelTitle", "heading"},
                { "noteCssClass", "alertType" },
                { "editorNotes", "message" },
                { "hideLabel", "hideLabel" },
                 { "noteRenderMode", "" },
            }) as JObject;
            // properties on contentment editor notes that don't exist in 
            config?.Add("icon", "icon-info color-black");
            config?.Add("hidePropertyGroup", "1");
            return config;

        }
    }
}
