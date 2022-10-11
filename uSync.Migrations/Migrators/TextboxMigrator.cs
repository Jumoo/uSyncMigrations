using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;
public class TextboxMigrator : SyncMigratorBase
{
    public override string[] Editors => new[] { "Umbraco.Textbox" };

    public override string GetEditorAlias(string editorAlias, string dabaseType)
        => "Umbraco.TextBox";

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
        => new TextboxConfiguration().MapPreValues(preValues);
}
