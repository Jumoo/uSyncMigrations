using Newtonsoft.Json;
using System.Xml.Linq;
using System.Xml.Serialization;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace uSync.Migrations.Migrators.Community.SeoChecker;

[SyncMigratorVersion(7, 8)]
[SyncMigrator("SEOChecker.RobotsTxt")]
public class SeoCheckerMetaRobotsToSeparateFields : SyncPropertyMigratorBase, ISyncPropertySplittingMigrator
{
	public IEnumerable<SplitPropertyContent> GetContentValues(SyncMigrationContentProperty migrationProperty, SyncMigrationContext context)
	{
		if (string.IsNullOrWhiteSpace(migrationProperty.Value))
		{
			yield break;
		}

		var serializer = new XmlSerializer(typeof(SEOCheckerMetaRobotValues));
		var content = (SEOCheckerMetaRobotValues?)serializer.Deserialize(new StringReader(migrationProperty.Value));

		if (content != null)
		{
			string index = string.IsNullOrWhiteSpace(content.Index) ? string.Empty : JsonConvert.SerializeObject(content.Index.AsEnumerableOfOne(), Formatting.Indented);
			string follow = string.IsNullOrWhiteSpace(content.Follow) ? string.Empty : JsonConvert.SerializeObject(content.Follow.AsEnumerableOfOne(), Formatting.Indented);

			yield return new SplitPropertyContent("robotsIndex", new XCData(index));
			yield return new SplitPropertyContent("robotsFollow", new XCData(follow));
		}
	}

	private readonly IEnumerable<NewDataTypeInfo> dataTypes = new NewDataTypeInfo[] {
		new NewDataTypeInfo(
			key: "robotsIndex".ToGuid(),
			alias: "robotsIndex",
			name: "Robots Index",
			editorAlias: UmbConstants.PropertyEditors.Aliases.DropDownListFlexible,
			databaseType: "Nvarchar",
			config: new DropDownFlexibleConfiguration()
			{
				Multiple = false,
				Items = new List<ValueListConfiguration.ValueListItem>()
				{
					new ValueListConfiguration.ValueListItem { Id = 1, Value = "index" },
					new ValueListConfiguration.ValueListItem { Id = 2, Value = "noindex" }
				}
			}
		),
		new NewDataTypeInfo(
			key: "robotsFollow".ToGuid(),
			alias: "robotsFollow",
			name: "Robots Follow",
			editorAlias: UmbConstants.PropertyEditors.Aliases.DropDownListFlexible,
			databaseType: "Nvarchar",
			config: new DropDownFlexibleConfiguration()
			{
				Multiple = false,
				Items = new List<ValueListConfiguration.ValueListItem>()
				{
					new ValueListConfiguration.ValueListItem { Id = 1, Value = "follow" },
					new ValueListConfiguration.ValueListItem { Id = 2, Value = "nofollow" }
				}
			}
		)
		};

	public IEnumerable<SplitPropertyInfo> GetSplitProperties(string contentTypeAlias, string propertyAlias, string propertyName, SyncMigrationContext context)
	{
		foreach (var dataTypeDefinition in dataTypes)
		{

			var dataType = context.DataTypes.GetFirstDefinition(dataTypeDefinition.Alias);
			if (dataType == null || dataType == Guid.Empty)
			{
				context.DataTypes.AddNewDataType(dataTypeDefinition);
				dataType = dataTypeDefinition.Key;
			}

			if (dataType.HasValue == false || dataType == Guid.Empty)
			{
				throw new Exception($"Failed to create required data types for {dataTypeDefinition.Name}.");
			}

			yield return new SplitPropertyInfo(dataTypeDefinition.Name, GeneratePropertyAlias(propertyAlias, dataTypeDefinition.Alias), dataTypeDefinition.EditorAlias, dataType.Value);
		}
	}

	public virtual string GeneratePropertyAlias(string originalPropertyAlias, string newPropertyAlias)
	{
		return $"{originalPropertyAlias}{newPropertyAlias.ToFirstUpper()}";
	}
}

[XmlRoot("SEOCheckerMetaRobotValues")]
public class SEOCheckerMetaRobotValues
{
	[XmlElement("index")]
	public string Index { get; set; }

	[XmlElement("follow")]
	public string Follow { get; set; }
}