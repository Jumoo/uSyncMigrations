using Umbraco.Cms.Core.PropertyEditors;

namespace uSync.Migrations.Migrators;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.UploadField, typeof(FileUploadConfiguration))]
internal class UploadFieldMigrator : SyncPropertyMigratorBase
{ }
