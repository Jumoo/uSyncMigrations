using Umbraco.Cms.Core.PropertyEditors;

namespace uSync.Migrations.Migrators;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.TinyMce, typeof(RichTextConfiguration))]
[SyncMigrator("Umbraco.TinyMCEv3")]
internal class RichTextBoxMigrator : SyncPropertyMigratorBase
{ }
