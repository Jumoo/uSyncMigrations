using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;
public class ColourPickerMigrator : SyncMigratorBase
{
    public override string[] Editors => new[] { "Umbraco.ColorPickerAlias" };

    public override string GetEditorAlias(string editorAlias, string dabaseType)
        => "Umbraco.ColorPicker";

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
    {
        var config = new ColorPickerConfiguration();

        int count = 0;
        foreach (var prevalue in preValues)
        {
            if (prevalue.Alias.InvariantEquals("useLabel"))
            {
                config.UseLabel = int.Parse(prevalue.Value) == 1;
            }
            else
            {
                if (prevalue.Value.DetectIsJson())
                {
                    var currentValue = JsonConvert.DeserializeObject<LegacyColourValue>(prevalue.Value);
                    if (currentValue != null)
                    {
                        var newValue = currentValue as ColorItemValue;

                        config.Items.Add(new ValueListConfiguration.ValueListItem
                        {
                            Id = currentValue.SortOrder,
                            Value = JsonConvert.SerializeObject(newValue)
                        });
                    }
                }
                else
                {
                    config.Items.Add(new ValueListConfiguration.ValueListItem
                    {
                        Id = count,
                        Value = prevalue.Value
                    });
                }
            }

            count++;
        }

        return config;
    }

    private class ColorItemValue
    {
        public string Value { get; set; }
        public string Label { get; set; }
    }

    private class LegacyColourValue : ColorItemValue
    {
        public int SortOrder { get; set; }
    }


}
