using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

internal class UploadFieldMigrator : SyncPropertyMigratorBase
{
    public override string[] Editors => new[] { UmbConstants.PropertyEditors.Aliases.UploadField };

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues, SyncMigrationContext context)
        => new FileUploadConfiguration().MapPreValues(preValues);
}
