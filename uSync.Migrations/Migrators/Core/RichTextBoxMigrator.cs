using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

internal class RichTextBoxMigrator : SyncPropertyMigratorBase
{
    public override string[] Editors => new[]
    {
        UmbConstants.PropertyEditors.Aliases.TinyMce,
        "Umbraco.TinyMCEv3",
    };

    public override string GetEditorAlias(string editorAlias, string databaseType, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.TinyMce;

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues, SyncMigrationContext context)
        => new RichTextConfiguration().MapPreValues(preValues);
}
