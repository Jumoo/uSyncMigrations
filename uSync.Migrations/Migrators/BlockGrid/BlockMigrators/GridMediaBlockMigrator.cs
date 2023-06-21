using Newtonsoft.Json;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

using uSync.Migrations.Context;
using uSync.Migrations.Legacy.Grid;

namespace uSync.Migrations.Migrators.BlockGrid.BlockMigrators;

public class GridMediaBlockMigrator : GridBlockMigratorSimpleBase, ISyncBlockMigrator
{
	public GridMediaBlockMigrator(IShortStringHelper shortStringHelper)
		 : base(shortStringHelper)
	{}

	public string[] Aliases => new[] { "media" };

	public override string GetEditorAlias(ILegacyGridEditorConfig editor) => "Media Picker";

	public override Dictionary<string, object> GetPropertyValues(GridValue.GridControl control, SyncMigrationContext context)
	{
		var properties = new Dictionary<string, object>();
		if (control.Value == null) return properties;

		var udiString = control.Value.Value<string>("udi");
		if (udiString == null) return properties;

		// 
		if (UdiParser.TryParse(udiString, out Udi? udi) && udi is GuidUdi guidUdi) {

			var values = new
			{
				key = Guid.NewGuid(),
				mediaKey = guidUdi.Guid
			}.AsEnumerableOfOne();

			properties.Add("media", JsonConvert.SerializeObject(values));
		}
		return properties;
	}
}


