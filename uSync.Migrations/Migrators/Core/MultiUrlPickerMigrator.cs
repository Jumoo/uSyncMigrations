using Umbraco.Cms.Core.PropertyEditors;

namespace uSync.Migrations.Migrators;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.MultiUrlPicker, typeof(MultiUrlPickerConfiguration))]
public class MultiUrlPickerMigrator : SyncPropertyMigratorBase
{ }
