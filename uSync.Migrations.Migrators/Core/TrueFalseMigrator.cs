using Umbraco.Cms.Core.PropertyEditors;

namespace uSync.Migrations.Migrators.Core;

[SyncMigrator(UmbEditors.Aliases.Boolean, typeof(TrueFalseConfiguration))]
public class TrueFalseMigrator : SyncPropertyMigratorBase
{ }
