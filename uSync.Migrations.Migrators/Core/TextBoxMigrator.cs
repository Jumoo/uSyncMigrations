using Umbraco.Cms.Core.PropertyEditors;

namespace uSync.Migrations.Migrators.Core;

[SyncMigrator(UmbEditors.Aliases.TextBox, typeof(TextboxConfiguration))]
public class TextBoxMigrator : SyncPropertyMigratorBase
{

    /// <summary>
    ///  in v8 the textbox changes from Textbox to TextBox, 
    /// </summary>
    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => UmbEditors.Aliases.TextBox;

}
