using Newtonsoft.Json;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

using uSync.Migrations.Context;
using uSync.Migrations.Legacy.Grid;

namespace uSync.Migrations.Migrators.BlockGrid.BlockMigrators;

public class GridMediaBlockMigrator : GridBlockMigratorSimpleBase, ISyncBlockMigrator
{
    private readonly IMediaService _mediaService;
    public GridMediaBlockMigrator(IShortStringHelper shortStringHelper, IMediaService mediaService)
         : base(shortStringHelper)
    {
        _mediaService = mediaService;
    }

    public string[] Aliases => new[] { "media" };

    public override string GetEditorAlias(ILegacyGridEditorConfig editor) => "Media Picker";

    public override Dictionary<string, object> GetPropertyValues(GridValue.GridControl control, SyncMigrationContext context)
    {
        var properties = new Dictionary<string, object>();
        if (control.Value == null) return properties;

        var udiString = control.Value.Value<string>("udi");

        Guid mediaKeyGuid = Guid.Empty;

        if (udiString == null)
        {
            var idValue = control.Value.Value<string>("id");

            if (string.IsNullOrWhiteSpace(idValue)) return properties;
            if (!int.TryParse(idValue, out var id)) return properties;

            var mediaItem = _mediaService.GetById(id);
            if (mediaItem == null) return properties;
            mediaKeyGuid = mediaItem.GetUdi().Guid;
        }
        else if (UdiParser.TryParse(udiString, out Udi? udi) && udi is GuidUdi guidUdi)
        {
            mediaKeyGuid = guidUdi.Guid;
        }

        var values = new
        {
            key = Guid.NewGuid(),
            mediaKey = mediaKeyGuid
        }.AsEnumerableOfOne();

        properties.Add("media", JsonConvert.SerializeObject(values));

        return properties;
    }
}


