using Umbraco.Cms.Core.Composing;
using uSync.Migrations.Context;
using uSync.Migrations.Migrators;
using uSync.Migrations.Migrators.Models;

namespace MyMigrations;

[SyncMigrator(Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.Grid)]
[HideFromTypeFinder] // hide from type if you don't want to automatically load this one (you have to replace in a composer)
// [SyncDefaultMigrator] // set it to default if you do load it and always want it to be the one you use. (can be overriden by preferred)
internal class GridToBlockListMigrator : SyncPropertyMigratorBase
{
    /// <summary>
    ///  the migrated editor alias.
    /// </summary>
    /// <remarks>
    ///  e.g if you are converting Umbraco.Grid to Umbraco.BlockGrid
    /// </remarks>
    public override string GetEditorAlias(SyncMigrationDataTypeProperty propertyModel, SyncMigrationContext context)
        => Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.BlockList;

    // TODO: Convert the grid config to block list grid. 
    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeModel, SyncMigrationContext context)
        => base.GetConfigValues(dataTypeModel, context);

    // TODO: Convert grid content to blocklist grid content
    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
        => base.GetContentValue(contentProperty, context);
}
