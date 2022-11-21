using Umbraco.Cms.Core.PropertyEditors;

namespace uSync.Migrations.Migrators;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.Boolean, typeof(TrueFalseConfiguration))]
public class TrueFalseMigrator : SyncPropertyMigratorBase
{ }
