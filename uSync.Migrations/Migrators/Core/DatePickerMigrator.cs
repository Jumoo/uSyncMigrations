using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

internal class DatePickerMigrator : SyncPropertyMigratorBase
{
    public override string[] Editors => new[]
    {
        "Umbraco.Date",
        UmbConstants.PropertyEditors.Aliases.DateTime,
    };

    public override string GetEditorAlias(string editorAlias, string databaseType, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.DateTime;

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues, SyncMigrationContext context)
        => new DateTimeConfiguration().MapPreValues(preValues);
}
