using Newtonsoft.Json.Linq;

namespace uSync.Migrations.Migrators.Community.TerraType;

[SyncMigrator("Terratype")]
public class TerratypeToGMapsMigrator : SyncPropertyMigratorBase
{
    private const string DefaultPosition = "55.406321,10.387015";
    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => "Our.Umbraco.GMaps";

    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var oldConfig = base.GetConfigValues(dataTypeProperty, context) as JObject;
        if (oldConfig == null) return new JObject();

        var config = JObject.FromObject(new
        {
            location = oldConfig.SelectToken("definition.position.datum") ?? DefaultPosition,
            zoom = oldConfig.SelectToken("zoom") ?? 12,
            maptype = "roadmap"
        });

        return config;
    }

    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        var oldStringValue = base.GetContentValue(contentProperty, context);
        var oldValue = JObject.Parse(oldStringValue ?? "{}");

        var zoom = oldValue.SelectToken("zoom") ?? "12";

        var latLng = oldValue.SelectToken("position.datum") ?? DefaultPosition;
        var parts = latLng.ToString().Split(',').Select(double.Parse).ToArray();


        var newValue = JObject.FromObject(new
        {
            address = new
            {
                coordinates = new
                {
                    lat = parts[0],
                    lng = parts[1]
                }
            },
            mapconfig = new
            {
                zoom = zoom,
                maptype = "roadmap",
                centerCoordinates = new
                {
                    lat = parts[0],
                    lng = parts[1]
                }
            }
        });

        return newValue.ToString();
    }
}
