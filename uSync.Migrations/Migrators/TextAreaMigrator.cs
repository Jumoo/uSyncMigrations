using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

public class TextAreaMigrator : SyncMigratorBase
{
    public override string[] Editors => new[]
    {
        "Umbraco.TextArea",
        "Umbraco.TextboxMultiple"
    };

    public override string GetEditorAlias(string editorAlias, string dabaseType)
        => "Umbraco.TextArea";

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
        => new TextAreaConfiguration().MapPreValues(preValues);
}
