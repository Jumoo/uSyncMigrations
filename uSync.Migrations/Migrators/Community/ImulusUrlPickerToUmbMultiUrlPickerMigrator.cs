using Newtonsoft.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Context;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Migrators.Models.UrlPicker;

namespace uSync.Migrations.Migrators;

[SyncMigrator("Imulus.UrlPicker")]
[SyncMigrator("Imulus.UrlPicker2")]
public class ImulusUrlPickerToUmbMultiUrlPickerMigrator : SyncPropertyMigratorBase
{
    public override object GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var config = new MultiUrlPickerConfiguration();
        return config;
    }
    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    => UmbConstants.PropertyEditors.Aliases.MultiUrlPicker;

    public override string GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        var urlPicker = JsonConvert.DeserializeObject<UrlPicker>(contentProperty.Value);

        var values = new Dictionary<string, string>();
        if (urlPicker != null)
        {
            values.Add("name", urlPicker.Value.Meta.Title);
            bool.TryParse(urlPicker.Value.Meta.NewWindow, out var newWindow);

            if (newWindow) { values.Add("target", "_blank"); }

            switch (urlPicker.Value.Type)
            {
                case UrlPickerTypes.Content:

                    var guid = context.GetKey(int.Parse(urlPicker.Value.TypeData.ContentId));
                    if (guid != Guid.Empty)
                    {
                        var contentUdi = Udi.Create(UmbConstants.UdiEntityType.Document, guid);
                        values.Add("udi", contentUdi.ToString());
                    }

                    break;
                case UrlPickerTypes.Media:

                    guid = context.GetKey(int.Parse(urlPicker.Value.TypeData.MediaId));
                    if (guid != Guid.Empty)
                    {
                        var contentUdi = Udi.Create(UmbConstants.UdiEntityType.Media, guid);
                        values.Add("udi", contentUdi.ToString());
                    }
                    break;

                case UrlPickerTypes.Url:
                    values.Add("url", urlPicker.Value.TypeData.Url);
                    break;

                default: break;
            }
        }
        return JsonConvert.SerializeObject(values);
    }

}