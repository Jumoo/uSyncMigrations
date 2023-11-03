using Umbraco.Cms.Core.PropertyEditors;

namespace uSync.Migrations.Migrators.Core;

[SyncMigrator(UmbEditors.Aliases.TextArea, typeof(TextAreaConfiguration), IsDefaultAlias = true)]
[SyncMigrator("Umbraco.TextboxMultiple")]
public class TextAreaMigrator : SyncPropertyMigratorBase
{ }

