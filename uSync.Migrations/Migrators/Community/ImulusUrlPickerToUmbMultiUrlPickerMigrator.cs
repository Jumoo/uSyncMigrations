using Newtonsoft.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

using uSync.Migrations.Context;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Migrators.Models.UrlPicker;

namespace uSync.Migrations.Migrators;

[SyncMigrator("Imulus.UrlPicker")]
[SyncMigrator("Imulus.UrlPicker2")]
public class ImulusUrlPickerToUmbMultiUrlPickerMigrator : SyncPropertyMigratorBase
{
    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.MultiUrlPicker;
    
    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var config = new MultiUrlPickerConfiguration();
        return config;
    }

    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (contentProperty?.Value == null) return contentProperty?.Value;

        var pickers = GetPickerValues(contentProperty.Value);
        if (!pickers.Any()) return contentProperty?.Value;


        var links = new List<MultiUrlPickerValueEditor.LinkDto>();

        foreach (var picker in pickers)
        {
            if (picker?.Meta == null || picker.TypeData == null) continue;

            var link = new MultiUrlPickerValueEditor.LinkDto
            {
                Name = picker.Meta.Title ?? ""
            };

            if (bool.TryParse(picker.Meta.NewWindow, out var newWindow))
            {
                if (newWindow) link.Target = "_blank";
            }

            switch (picker.Type)
            {
                case UrlPickerTypes.Content:
                    var contentLink = GetUdiValueFromIntString(picker.TypeData.ContentId, picker.Type, context);
                    link.Udi = contentLink ?? null;
                    break;
                case UrlPickerTypes.Media:
                    var mediaLink = GetUdiValueFromIntString(picker.TypeData.MediaId, picker.Type, context);
                    link.Udi = mediaLink ?? null;
                    break;
                case UrlPickerTypes.Url:
                    link.Url = picker.TypeData.Url ?? null;
                    break;

                default: break;
            }

            links.Add(link);
        }
        return JsonConvert.SerializeObject(links, Formatting.Indented);
    }


    private IEnumerable<UrlPickerValue> GetPickerValues(string? contentValue)
    {
        if (contentValue == null) return Enumerable.Empty<UrlPickerValue>();

        if (contentValue.StartsWith("["))
        {
            return JsonConvert.DeserializeObject<IEnumerable<UrlPickerValue>>(contentValue) ?? Enumerable.Empty<UrlPickerValue>();
        }

        return JsonConvert.DeserializeObject<UrlPicker>(contentValue)?.Value.AsEnumerableOfOne() ?? Enumerable.Empty<UrlPickerValue>();
    }

    private GuidUdi? GetUdiValueFromIntString(string? idValue, UrlPickerTypes pickerType, SyncMigrationContext context) 
    {
        if (string.IsNullOrWhiteSpace(idValue)) return null;
        
        if (!int.TryParse(idValue, out int valueIntId)) { return null; }       

        var guid = context.GetKey(valueIntId);
        if (guid == Guid.Empty) return null;
        
        var entityType = pickerType == UrlPickerTypes.Content ? UmbConstants.UdiEntityType.Document : UmbConstants.UdiEntityType.Media;
        return Udi.Create(entityType, guid) as GuidUdi;
    }

}