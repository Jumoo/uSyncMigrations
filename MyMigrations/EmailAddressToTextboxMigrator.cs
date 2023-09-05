using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators;
using uSync.Migrations.Migrators.Models;

namespace MyMigrations
{
    [SyncMigrator("Umbraco.EmailAddress")]
    public class EmailAddressToTextboxMigrator : SyncPropertyMigratorBase
    {
        public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
      => Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.TextBox;
        
    }
}
