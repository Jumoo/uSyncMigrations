using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;
internal class RelatedLinksToMultiUrlPickerMigrator : SyncMigratorBase
{
    public override string[] Editors => new[] {
        "Umbraco.RelatedLinks",
        "Umbraco.RelatedLinks2"
    };

    public override string GetEditorAlias(string editorAlias, string dabaseType)
        => "Umbraco.MultiUrlPicker";

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
    {
        var config = new MultiUrlPickerConfiguration();
        var maxValue = preValues.GetPreValueOrDefault("max", -1);
        if (maxValue != -1) config.MaxNumber = maxValue;

        return config;
    }
}
