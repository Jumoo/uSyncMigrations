using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Umbraco.Cms.Core;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Models.Property;

namespace uSync.Migrations.Migrators.Community.Meganav;

/// <summary>
/// Based on https://github.com/callumbwhyte/meganav/tree/v9/dev/src/Our.Umbraco.Meganav/Migrations
/// </summary>
[SyncMigrator("Cogworks.Meganav")]
[SyncMigrator("Meganav")]
[SyncDefaultMigrator]
public class MeganavToMeganavMigrator : SyncPropertyMigratorBase
{
	public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
		=> "Our.Umbraco.Meganav";

	public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
	{
		var oldStringValue = base.GetContentValue(contentProperty, context);
		var data = JToken.Parse(string.IsNullOrEmpty(oldStringValue) ? "{}" : oldStringValue);

		var entities = ConvertToEntity(data, context);
		var value = JsonConvert.SerializeObject(entities, new JsonSerializerSettings() {  });
		
		return value;
	}

	private IEnumerable<MeganavEntity> ConvertToEntity(JToken data, SyncMigrationContext context)
	{
		foreach (var item in data.Children())
		{
			var entity = new MeganavEntity
			{
				Title = item.Value<string>("title"),
				Target = item.Value<string>("target"),
				Visible = !item.Value<bool>("naviHide")
			};


			var id = item.Value<int>("id");

			UdiParser.TryParse(item.Value<string>("udi"), out Udi udi);

			if (id > 0 || udi != null)
			{
				if (udi is GuidUdi guidUdi)
				{
					// UDI is healthy

					// UNDONE: the original source still obtains the object here but I think it's not needed.
					// All I can see it'd do is check the node still exists, which is handled by the editor anyway.
				}
				else
				{
					// convert ID to key
					guidUdi = new GuidUdi(UmbConstants.UdiEntityType.Document, context.GetKey(id));
				}

				entity.Udi = guidUdi;
			}
			else
			{
				entity.Url = item.Value<string>("url");
			}

			var children = item.Value<JArray>("children");

			if (children != null)
			{
				entity.Children = ConvertToEntity(children, context);
			}

			var ignoreProperties = new[]
			{
				"id", "key", "udi", "name", "title", "description", "target", "url", "children", "icon", "published",
				"naviHide", "culture"
			};

			var settings = item.ToObject<IDictionary<string, object>>();

			settings.RemoveAll(x => ignoreProperties.InvariantContains(x.Key));

			entity.Settings = settings;

			yield return entity;
		}
	}

	[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
	internal class MeganavEntity
	{
		[DataMember(Name = "title")]
		public string Title { get; set; }

		[DataMember(Name = "url")]
		public string Url { get; set; }

		[DataMember(Name = "target")]
		public string Target { get; set; }

		[DataMember(Name = "visible")]
		public bool Visible { get; set; } = true;

		[DataMember(Name = "udi")]
		public GuidUdi Udi { get; set; }

		[DataMember(Name = "itemTypeId")]
		public Guid? ItemTypeId { get; set; }

		[DataMember(Name = "settings")]
		public IDictionary<string, object> Settings { get; set; }

		[DataMember(Name = "children")]
		public IEnumerable<MeganavEntity> Children { get; set; } = new List<MeganavEntity>();
	}
}
