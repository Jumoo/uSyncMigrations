using Umbraco.Cms.Core.PropertyEditors;

namespace uSync.Migrations.Migrators;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.MultipleTextstring, typeof(MultipleTextStringConfiguration))]
internal class MultipleTextStringMigrator : SyncPropertyMigratorBase
{ }
