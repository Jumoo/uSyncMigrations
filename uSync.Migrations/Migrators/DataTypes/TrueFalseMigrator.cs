using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.DataTypes;
internal class TrueFalseMigrator : DataTypeMigratorBase
{
    public override string[] Editors => new[] { "Umbraco.TrueFalse" };

    public override object GetConfigValues(SyncDataTypeInfo dataTypeInfo)
        => dataTypeInfo.MapPreValues(new TrueFalseConfiguration());
}

internal class SwitcherToTrueFalseMigrator : DataTypeMigratorBase
{
    public override string[] Editors => new[] { "Our.Umbraco.Switcher" };

    public override object GetConfigValues(SyncDataTypeInfo dataTypeInfo)
        => dataTypeInfo.MapPreValues(new TrueFalseConfiguration(),
            new Dictionary<string, string>
            {
                { "hideLabel", nameof(TrueFalseConfiguration.ShowLabels) },
                { "onLabelText", nameof(TrueFalseConfiguration.LabelOn) },
                { "offLabelText", nameof(TrueFalseConfiguration.LabelOff) },
                { "switchOn", nameof(TrueFalseConfiguration.Default) }
            });
}
