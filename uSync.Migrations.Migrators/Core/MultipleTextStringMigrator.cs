using Umbraco.Cms.Core.PropertyEditors;

namespace uSync.Migrations.Migrators.Core;

[SyncMigrator(UmbEditors.Aliases.MultipleTextstring, typeof(MultipleTextStringConfiguration))]
public class MultipleTextStringMigrator : SyncPropertyMigratorBase
{ }
