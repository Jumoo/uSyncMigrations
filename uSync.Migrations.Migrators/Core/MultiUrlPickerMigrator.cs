using Umbraco.Cms.Core.PropertyEditors;

namespace uSync.Migrations.Migrators.Core;

[SyncMigrator(UmbEditors.Aliases.MultiUrlPicker, typeof(MultiUrlPickerConfiguration))]
public class MultiUrlPickerMigrator : SyncPropertyMigratorBase
{ }
