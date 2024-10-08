namespace uSync.Migrations.Migrators.Community.Meganav;

[SyncMigrator("Cogworks.Meganav")]
[SyncDefaultMigrator]
public class MeganavToMeganavMigrator : SyncPropertyMigratorBase
{
    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => "Our.Umbraco.Meganav";
}
