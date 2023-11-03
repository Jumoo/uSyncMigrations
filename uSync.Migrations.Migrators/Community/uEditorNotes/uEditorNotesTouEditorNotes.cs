using uSync.Migrations.Core.Extensions;

namespace uSync.Migrations.Migrators.Community.uEditorNotes;

[SyncMigrator("tooorangey.EditorNotes")]
[SyncDefaultMigrator]
public class uEditorNotesTouEditorNotesMigrator : SyncPropertyMigratorBase
{
    //alias is the same
    //content is the same
    // prevalue config is differently done but are 1-1 naming wise
    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => dataTypeProperty.PreValues.ConvertPreValuesToJson(false);
}
