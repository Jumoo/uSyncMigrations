using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;
internal class TrueFalseMigrator : SyncMigratorBase
{
    public override string[] Editors => new[] { "Umbraco.TrueFalse" };

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
        => new TrueFalseConfiguration().MapPreValues(preValues);
}

internal class SwitcherToTrueFalseMigrator : SyncMigratorBase
{
    public override string[] Editors => new[] { "Our.Umbraco.Switcher" };

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
        => new TrueFalseConfiguration().MapPreValues(
            preValues,
            new Dictionary<string, string>
            {
                { "hideLabel", nameof(TrueFalseConfiguration.ShowLabels) },
                { "onLabelText", nameof(TrueFalseConfiguration.LabelOn) },
                { "offLabelText", nameof(TrueFalseConfiguration.LabelOff) },
                { "switchOn", nameof(TrueFalseConfiguration.Default) }
            });
}
