using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Extensions;
using uSync.Migrations.Models;


namespace uSync.Migrations.Migrators;
internal class DatePickerMigrator : SyncMigratorBase
{
    public override string[] Editors => new[] { "Umbraco.Date", "Umbraco.DateTime" };

    public override string GetEditorAlias(string editorAlias, string dabaseType)
        => "Umbraco.DateTime";

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
        => new DateTimeConfiguration().MapPreValues(preValues);
}
