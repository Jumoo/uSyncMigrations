using Umbraco.Cms.Core.PropertyEditors;

namespace uSync.Migrations.Migrators;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.Grid, typeof(GridConfiguration))]
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