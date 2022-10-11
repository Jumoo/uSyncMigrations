using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;
internal class RichTextBoxMigrator : SyncMigratorBase
{
    public override string[] Editors => new[] { "Umbraco.TinyMCEv3" };

    public override string GetEditorAlias(string editorAlias, string dabaseType)
        => "Umbraco.TinyMCE";

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
        => new RichTextConfiguration().MapPreValues(preValues);
}
