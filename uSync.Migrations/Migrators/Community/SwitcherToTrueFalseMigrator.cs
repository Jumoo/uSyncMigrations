using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

internal class SwitcherToTrueFalseMigrator : SyncPropertyMigratorBase
{
    public override string[] Editors => new[] { "Our.Umbraco.Switcher" };

    public override string GetEditorAlias(string editorAlias, string databaseType, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.Boolean;

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues, SyncMigrationContext context)
    {
        return new TrueFalseConfiguration().MapPreValues(preValues, new Dictionary<string, string>
        {
            { "hideLabel", nameof(TrueFalseConfiguration.ShowLabels) },
            { "onLabelText", nameof(TrueFalseConfiguration.LabelOn) },
            { "offLabelText", nameof(TrueFalseConfiguration.LabelOff) },
            { "switchOn", nameof(TrueFalseConfiguration.Default) }
        });
    }
}