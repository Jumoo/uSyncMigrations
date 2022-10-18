using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

public class TextAreaMigrator : SyncPropertyMigratorBase
{
    public override string[] Editors => new[]
    {
        UmbConstants.PropertyEditors.Aliases.TextArea,
        "Umbraco.TextboxMultiple",
    };

    public override string GetEditorAlias(string editorAlias, string databaseType, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.TextArea;

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues, SyncMigrationContext context)
        => new TextAreaConfiguration().MapPreValues(preValues);
}
