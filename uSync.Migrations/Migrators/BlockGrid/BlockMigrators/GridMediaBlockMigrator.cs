using Newtonsoft.Json;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Grid;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

using uSync.Migrations.Composing;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.BlockGrid.BlockMigrators;

public class GridMediaBlockMigrator : GridBlockMigratorSimpleBase, ISyncBlockMigrator
{
	private readonly SyncPropertyMigratorCollection _propertyMigrators;
	public GridMediaBlockMigrator(IShortStringHelper shortStringHelper, SyncPropertyMigratorCollection propertyMigrators)
		 : base(shortStringHelper)
	{
		_propertyMigrators = propertyMigrators;
	}

	public string[] Aliases => new[] { "media" };

	public override string GetEditorAlias(IGridEditorConfig editor) => "Media Picker";

	public override Dictionary<string, object> GetPropertyValues(GridValue.GridControl control, SyncMigrationContext context)
	{
		var properties = new Dictionary<string, object>();
		if (control.Value == null) return properties;

		// 
		if (UdiParser.TryParse(control.Value?.Value<string>("udi"), out Udi udi) && udi is GuidUdi guidUdi) {

			var values = new
			{
				key = Guid.NewGuid(),
				mediaKey = guidUdi.Guid
			}.AsEnumerableOfOne();

			properties.Add("media", JsonConvert.SerializeObject(values));
		}
		return properties;

		//var mediaPickerMigrator = _propertyMigrators.FirstOrDefault(x => x.Editors
		//	.InvariantContains(UmbConstants.PropertyEditors.Aliases.MediaPicker));

		//if (mediaPickerMigrator != null)
		//{
		//	var contentValue = mediaPickerMigrator.GetContentValue(new SyncMigrationContentProperty
		//		(UmbConstants.PropertyEditors.Aliases.MediaPicker3, control.Value.ToString()), context);

		//	properties.Add("media", contentValue);
		//}
		//else
		//{
		//	var mediaUdi = control.Value?.Value<string>("udi");
		//	if (mediaUdi != null) properties.Add("media", mediaUdi);
		//}

		//return properties;

	}
}


