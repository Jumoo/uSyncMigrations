using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

internal class NestedContentMigrator : SyncPropertyMigratorBase
{
    public override string[] Editors => new[]
    {
        "Our.Umbraco.NestedContent",
        UmbConstants.PropertyEditors.Aliases.NestedContent
    };

    public override string GetEditorAlias(string editorAlias, string databaseType, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.NestedContent;

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues, SyncMigrationContext context)
        => new NestedContentConfiguration().MapPreValues(preValues);
}
