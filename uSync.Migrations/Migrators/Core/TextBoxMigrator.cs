using Umbraco.Cms.Core.PropertyEditors;

namespace uSync.Migrations.Migrators;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.TextBox, typeof(TextboxConfiguration))]
public class TextBoxMigrator : SyncPropertyMigratorBase
{ }
