using Newtonsoft.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;
using uSync.Migrations.Migrators.Community.GibeLinkPicker.Models;

namespace uSync.Migrations.Migrators.Community.GibeLinkPicker
{
    [SyncMigrator("Gibe.LinkPicker")]
    public class GibeLinkPickerToMultiUrlPickerMigrator : SyncPropertyMigratorBase
    {
        public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
                => UmbConstants.PropertyEditors.Aliases.MultiUrlPicker;

        public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        {
            var config = new MultiUrlPickerConfiguration
            {
                MaxNumber = 1
            };
            return config;
        }

        public override string GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
        {
            if (contentProperty?.Value == null)
            {
                return contentProperty?.Value;
            }

            var gibeLinkPickers = GetPickerValues(contentProperty.Value).ToArray();

            if (!gibeLinkPickers.Any())
            {
                return contentProperty.Value;
            }

            var links = new List<MultiUrlPickerValueEditor.LinkDto>();

            foreach (var picker in gibeLinkPickers)
            {
                var link = new MultiUrlPickerValueEditor.LinkDto
                {
                    Name = picker?.Name ?? string.Empty,
                    Url = picker?.Url ?? string.Empty,
                    Udi = picker?.Uid != null ? new GuidUdi(UmbConstants.UdiEntityType.Document, Guid.Parse(picker.Uid)) : null,
                };

                if (picker?.Target == "_blank")
                {
                    link.Target = picker.Target;
                }

                links.Add(link);
            }
            return JsonConvert.SerializeObject(links, Formatting.Indented);
        }

        private IEnumerable<GibeLinkPickerData> GetPickerValues(string? contentValue)
        {
            if (contentValue == null)
            {
                return Enumerable.Empty<GibeLinkPickerData>();
            }

            if (contentValue.StartsWith("["))
            {
                return JsonConvert.DeserializeObject<IEnumerable<GibeLinkPickerData>>(contentValue)
                       ?? Enumerable.Empty<GibeLinkPickerData>();
            }
            return JsonConvert.DeserializeObject<GibeLinkPickerData>(contentValue)?.AsEnumerableOfOne()
                   ?? Enumerable.Empty<GibeLinkPickerData>();
        }
    }
}

