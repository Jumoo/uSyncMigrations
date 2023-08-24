using Newtonsoft.Json;
using Umbraco.Extensions;
using uSync.Migrations.Context;
using uSync.Migrations.Migrators;
using uSync.Migrations.Migrators.Models;

namespace MyMigrations.DTGEMigrator;

[SyncMigrator("DTGE." + Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.CheckBoxList)]

public class DTGECheckboxListMigrator : CheckboxListMigrator
{
    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (contentProperty.Value == null)
            return null;

        if (!contentProperty.Value.DetectIsJson())
            return JsonConvert.SerializeObject(contentProperty.Value.ToDelimitedList());

        DTGEPrevaluesMap prevaluesMap = new DTGEPrevaluesMap(context);

        List<string> outputValues = new List<string>();
        IList<string>? inputValues = JsonConvert.DeserializeObject<List<string>>(contentProperty.Value);
        foreach (var inputVal in inputValues)
        {
            if (Int32.TryParse(inputVal, out int valueId) &&
                prevaluesMap.HasValue(valueId))
            {
                string outputVal = prevaluesMap.GetValue(valueId);
                outputValues.Add(outputVal);
            }
            else
            {
                outputValues.Add(inputVal);
            }
        }

        return JsonConvert.SerializeObject(outputValues);
    }
}