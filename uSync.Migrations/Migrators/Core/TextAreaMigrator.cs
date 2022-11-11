using Umbraco.Cms.Core.PropertyEditors;

namespace uSync.Migrations.Migrators;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.TextArea, typeof(TextAreaConfiguration), IsDefaultAlias = true)]
[SyncMigrator("Umbraco.TextboxMultiple")]
public class TextAreaMigrator : SyncPropertyMigratorBase
{ }

