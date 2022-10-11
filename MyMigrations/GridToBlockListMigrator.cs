using uSync.Migrations;
using uSync.Migrations.Models;

namespace MyMigrations;
internal class GridToBlockListMigrator : SyncMigratorBase
{
    /// <summary>
    ///  list of datatype editors that this migrator works on
    /// </summary>
    public override string[] Editors => new[]
    {
        "Umbraco.Grid"
    };

    /// <summary>
    ///  the migrated editor alias. 
    /// </summary>
    /// <remarks>
    ///  e.g if you are converting Umbraco.Grid to Umbraco.BlockGrid
    /// </remarks>
    public override string GetEditorAlias(string editorAlias, string dabaseType)
        => "Umbraco.BlockGrid";

    // TODO: Convert the grid config to block list grid. 
    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
        => base.GetConfigValues(editorAlias, databaseType, preValues);

    // TODO: Convert grid content to blocklist grid content
    public override string GetContentValue(string editorAlias, string value)
        => base.GetContentValue(editorAlias, value);
}
