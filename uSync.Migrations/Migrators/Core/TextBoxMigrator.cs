using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Context;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.TextBox, typeof(TextboxConfiguration))]
public class TextBoxMigrator : SyncPropertyMigratorBase
{

    /// <summary>
    ///  in v8 the textbox changes from Textbox to TextBox, 
    /// </summary>
    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.TextBox;

}
