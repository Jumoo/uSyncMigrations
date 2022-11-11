using uSync.Migrations.Migrators;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Models;

namespace MyMigrations;

internal class GridToBlockListMigrator : SyncPropertyMigratorBase
{
    /// <summary>
    ///  list of datatype editors that this migrator works on
    /// </summary>
    public override string[] Editors => new[] { Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.Grid };

    /// <summary>
    ///  the migrated editor alias.
    /// </summary>
    /// <remarks>
    ///  e.g if you are converting Umbraco.Grid to Umbraco.BlockGrid
    /// </remarks>
    public override string GetEditorAlias(SyncMigrationDataTypeProperty propertyModel, SyncMigrationContext context)
        => Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.BlockList;

    // TODO: Convert the grid config to block list grid. 
    public override object GetConfigValues(SyncMigrationDataTypeProperty dataTypeModel, SyncMigrationContext context)
        => base.GetConfigValues(dataTypeModel, context);

    // TODO: Convert grid content to blocklist grid content
    public override string GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
        => base.GetContentValue(contentProperty, context);
}
