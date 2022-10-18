using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.Core;

internal class MemberPickerMigrator : SyncPropertyMigratorBase
{
    public override string[] Editors => new[]
    {
        UmbConstants.PropertyEditors.Aliases.MemberPicker,
        "Umbraco.MemberPicker2",
    };

    public override string GetEditorAlias(string editorAlias, string databaseType, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.MemberPicker;
}
