using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Migrators;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Models;

namespace uSyncMigrationSite;

[SyncMigrator("Novicell.FocalPointImage")]
public class FocalPointImageMigrator : SyncPropertyMigratorBase
{
    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.MediaPicker3;

    public override object GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        return new MediaPicker3Configuration
        {
            Multiple = false,
            EnableLocalFocalPoint = true,
        };
    }

    public override string GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (string.IsNullOrWhiteSpace(contentProperty.Value))
        {
            return string.Empty;
        }
        
        var source = JObject.Parse(contentProperty.Value);
        
        var mediaKey = Guid.Empty;
        
        if (source.TryGetValue("udi", out var udiValue) &&
            UdiParser.TryParse(udiValue.Value<string>() ?? string.Empty, out var udi) &&
            udi is GuidUdi guidUdi)
        {
            mediaKey = guidUdi.Guid;
        }
        else if (source.TryGetValue("key", out var keyValue) &&
                 Guid.TryParse(keyValue.Value<string>() ?? string.Empty, out var key))
        {
            mediaKey = key;
        }

        if (mediaKey == Guid.Empty)
        {
            if (source.TryGetValue("id", out var idValue) && int.TryParse(idValue.Value<string>(), out var id))
            {
                // TODO : get media from id
            }
            
            return string.Empty;
        }

        if (mediaKey == Guid.Empty)
        {
            return string.Empty;
        }
        
        var value = new Dictionary<string, object>
        {
            { "key", Guid.NewGuid() },
            { "mediaKey", mediaKey },
        };

        if (source.TryGetValue("focalPoint", out var focalPointValue))
        {
            var left = focalPointValue.Value<decimal>("left");
            var top = focalPointValue.Value<decimal>("top");

            if (top != 0.5m || left != 0.5m)
            {
                value["focalPoint"] = new
                {
                    left,
                    top,
                };
            }
        }
        
        return JsonConvert.SerializeObject(new[] { value });
    }
}