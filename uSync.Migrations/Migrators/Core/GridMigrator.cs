using Umbraco.Cms.Core.PropertyEditors;

namespace uSync.Migrations.Migrators;

/// <summary>
///  The grid migrator will need to be fleshed out
/// </summary>
/// <remarks>
///  in all likelyhood you will want to turn grids into blockgrids or lists.
///  
///  for that there will need to be a set of existing doctypes on a site that mimic the 
///  grid configurations you might have,
///  
///  what we might do. is add those in prep if they don't exist ? 
///  e.g a doctype for heading, rich text, quote (block grid adds these as demo types)
///  
///  there will likely then also need to be some config, so you can map them, 
///  
///  and then in the mapping if you have custom things (like DTGE) it will need a migrator too.
/// </remarks>
[SyncMigrator(UmbConstants.PropertyEditors.Aliases.Grid, typeof(GridConfiguration), IsDefaultAlias = true)]
public class GridMigrator : SyncPropertyMigratorBase
{
    // TODO: [KJ] This only really matters if there are custom things and they need config.

    //public override string GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    //{
    //    if (contentProperty.Value == null) return string.Empty;

    //    var grid = JsonConvert.DeserializeObject<GridValue>(contentProperty.Value);
    //    if (grid == null) return contentProperty.Value;

    //    var controls = grid.Sections.SelectMany(s => s.Rows.SelectMany(r => r.Areas.SelectMany(a => a.Controls);

    //    foreach (var control in controls)
    //    {
    //        var gridAlias = control.Editor.Alias;
    //    }

    //    return string.Empty;
    //}

}