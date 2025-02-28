using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace uSync.Migrations.Core.Legacy.Grid;

/// <summary>
///  legacy grid structure., 
/// </summary>
/// <remarks>
///  this is being removed from the core in v14, but we still need it 
///  to actually do the migration.
/// </remarks>
public class LegacyGridValue
{
    public class LegacyGridSection
    {
        [JsonProperty("grid")]
        public string? Grid { get; set; }

        [JsonProperty("rows")]
        public IEnumerable<LegacyGridRow> Rows { get; set; }
    }

    public class LegacyGridRow
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("areas")]
        public IEnumerable<LegacyGridArea> Areas { get; set; }

        [JsonProperty("styles")]
        public JToken? Styles { get; set; }

        [JsonProperty("config")]
        public JToken? Config { get; set; }
    }

    public class LegacyGridArea
    {
        [JsonProperty("grid")]
        public string? Grid { get; set; }

        [JsonProperty("controls")]
        public IEnumerable<LegacyGridControl> Controls { get; set; }

        [JsonProperty("styles")]
        public JToken? Styles { get; set; }

        [JsonProperty("config")]
        public JToken? Config { get; set; }
    }

    public class LegacyGridControl
    {
        [JsonProperty("value")]
        public JToken? Value { get; set; }

        [JsonProperty("editor")]
        public LegacyGridEditor Editor { get; set; }

        [JsonProperty("styles")]
        public JToken? Styles { get; set; }

        [JsonProperty("config")]
        public JToken? Config { get; set; }
    }

    public class LegacyGridEditor
    {
        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("view")]
        public string? View { get; set; }
    }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("sections")]
    public IEnumerable<LegacyGridSection> Sections { get; set; }
}