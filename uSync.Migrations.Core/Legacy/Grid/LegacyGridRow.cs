using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace uSync.Migrations.Core.Legacy.Grid;

/// <summary>
///  leagacy grid structure., 
/// </summary>
/// <remarks>
///  this is being removed from the core in v14, but we still need it 
///  to actually do the migration.
/// </remarks>
public class LegacyGridValue
{
    public class GridSection
    {
        [JsonProperty("grid")]
        public string? Grid { get; set; }

        [JsonProperty("rows")]
        public IEnumerable<GridRow> Rows { get; set; }
    }

    public class GridRow
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("areas")]
        public IEnumerable<GridArea> Areas { get; set; }

        [JsonProperty("styles")]
        public JToken? Styles { get; set; }

        [JsonProperty("config")]
        public JToken? Config { get; set; }
    }

    [Obsolete("The grid is obsolete, will be removed in V13")]
    public class GridArea
    {
        [JsonProperty("grid")]
        public string? Grid { get; set; }

        [JsonProperty("controls")]
        public IEnumerable<GridControl> Controls { get; set; }

        [JsonProperty("styles")]
        public JToken? Styles { get; set; }

        [JsonProperty("config")]
        public JToken? Config { get; set; }
    }

    [Obsolete("The grid is obsolete, will be removed in V13")]
    public class GridControl
    {
        [JsonProperty("value")]
        public JToken? Value { get; set; }

        [JsonProperty("editor")]
        public GridEditor Editor { get; set; }

        [JsonProperty("styles")]
        public JToken? Styles { get; set; }

        [JsonProperty("config")]
        public JToken? Config { get; set; }
    }

    [Obsolete("The grid is obsolete, will be removed in V13")]
    public class GridEditor
    {
        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("view")]
        public string? View { get; set; }
    }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("sections")]
    public IEnumerable<GridSection> Sections { get; set; }
}