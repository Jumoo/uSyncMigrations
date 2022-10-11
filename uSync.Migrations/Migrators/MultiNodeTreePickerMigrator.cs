using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using uSync.Migrations.Models;

using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Extensions;

namespace uSync.Migrations.Migrators;
internal class MultiNodeTreePickerMigrator : SyncMigratorBase
{
    public override string[] Editors => new[]
    {
        "Umbraco.MultiNodeTreePicker"
    };

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
    {
        var picker = new MultiNodePickerConfiguration();
        return picker.MapPreValues(preValues, new Dictionary<string, string>
        {
            { "filter", nameof(picker.Filter) },
            { "minNumber", nameof(picker.MinNumber) },
            { "maxNumber", nameof(picker.MaxNumber) },
            { "showOpenButton", nameof(picker.ShowOpen) },
            { "startNode", nameof(picker.Filter) }
        });
    }
}
