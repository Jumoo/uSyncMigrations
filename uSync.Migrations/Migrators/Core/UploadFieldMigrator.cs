using Umbraco.Cms.Core.PropertyEditors;

namespace uSync.Migrations.Migrators;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.UploadField, typeof(FileUploadConfiguration))]
public class UploadFieldMigrator : SyncPropertyMigratorBase
{ }
