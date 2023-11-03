using Umbraco.Cms.Core.PropertyEditors;

namespace uSync.Migrations.Migrators.Core;

[SyncMigrator(UmbEditors.Aliases.UploadField, typeof(FileUploadConfiguration))]
public class UploadFieldMigrator : SyncPropertyMigratorBase
{ }
