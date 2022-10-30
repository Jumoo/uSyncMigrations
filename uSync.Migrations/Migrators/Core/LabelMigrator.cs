using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

internal class LabelMigrator : SyncPropertyMigratorBase
{
    public override string[] Editors => new[] { "Umbraco.NoEdit" };

    public override string GetEditorAlias(string editorAlias, string databaseType, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.Label;

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues, SyncMigrationContext context)
    {
        return new LabelConfiguration
        {
            ValueType = preValues.GetPreValueOrDefault(UmbConstants.PropertyEditors.ConfigurationKeys.DataValueType, ValueTypes.String)
        };
    }
}
