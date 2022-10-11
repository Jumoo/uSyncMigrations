using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;
internal class NestedContentMigrator : SyncMigratorBase
{
    public override string[] Editors => new[]
    {
        "Umbraco.NestedContent"
    };

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
        => new NestedContentConfiguration().MapPreValues(preValues);
}
