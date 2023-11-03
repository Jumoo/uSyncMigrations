using uSync.Migrations.Core.Context;
using uSync.Migrations.Core.Migrators;
using uSync.Migrations.Core.Migrators.Models;

namespace MyMigrations;

[SyncMigrator("Umbraco.EmailAddress")]
public class EmailAddressToTextboxMigrator : SyncPropertyMigratorBase
{
    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
  => Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.TextBox;

}
