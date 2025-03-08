namespace uSync.Migrations.Migrators.Core;

/// <summary>
///  things that don't ordinarily need a migrator. 
/// </summary>
[SyncMigrator(UmbEditors.Aliases.MediaPicker3)]
[SyncMigrator(UmbEditors.Aliases.BlockGrid)]
[SyncMigrator(UmbEditors.Aliases.BlockList)]
[SyncMigrator(UmbEditors.Aliases.Label)]
[SyncMigrator(UmbEditors.Aliases.ContentPicker)]
public class NullMigrator : SyncPropertyMigratorBase
{
	public override int[] Versions => new int[] { 8 };
}
