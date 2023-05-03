using Newtonsoft.Json.Linq;
using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators;

[SyncMigrator("Terratype")]
public class TerratypeToOpenStreetmapMigrator : SyncPropertyMigratorBase
{
    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => "Bergmania.OpenStreetMap";

    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var config = new JObject();

        config.Add("allowClear", true);
        config.Add("defaultPosition", GetPosition(dataTypeProperty.PreValues?.GetPreValueOrDefault("definition", string.Empty) ?? string.Empty));
        config.Add("showCoordinates", false);
        config.Add("showSearch", false);
        config.Add("tileLayer", "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png");
        config.Add("tileLayerAttribution", "Map data © OpenStreetMap contributors");

        return config;
    }

    public JToken GetPosition(string jsonConfig)
    {
        var defaultJson = JObject.Parse(DEFAULT_POSITIONVALUE);

        if (string.IsNullOrWhiteSpace(jsonConfig)) return defaultJson;

        var terraTypeJson = JObject.Parse(jsonConfig);
        var position = terraTypeJson.Value<JObject>("position");
        if (position != null)
        {
            var cords = position.Value<string>("datum");
            if (!string.IsNullOrWhiteSpace(cords))
            {
                var xy = cords.Split(",");
                if (xy.Length == 2)
                {
                    defaultJson["marker"]!["latitude"] = decimal.Parse(xy[0]);
                    defaultJson["marker"]!["longitude"] = decimal.Parse(xy[1]);
                }
            }
        }

        return defaultJson;
    }

    private string DEFAULT_POSITIONVALUE = @"{
        ""boundingBox"": {
            ""northEastCorner"": { ""latitude"": 55.4331042620063, ""longitude"": 10.4534912109375 },
            ""southWestCorner"": { ""latitude"": 55.3648687459637, ""longitude"": 10.3075790405273 }
        },
        ""marker"": { ""latitude"": 55.4062262211674, ""longitude"": 10.388377904892 },
        ""zoom"": 12
    }";
}
