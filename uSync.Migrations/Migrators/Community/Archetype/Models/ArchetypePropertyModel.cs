using Newtonsoft.Json;

namespace Archetype.Models
{
    /// <summary>
    /// Model that represents a stored property in Archetype.
    /// </summary>
    public class ArchetypePropertyModel
    {
        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("value")]
        public object Value { get; set; }

        [JsonProperty("propertyEditorAlias")]
        public string PropertyEditorAlias { get; internal set; }

        [JsonProperty("dataTypeId")]
        public int DataTypeId { get; internal set; }

        [JsonProperty("dataTypeGuid")]
        internal string DataTypeGuid { get; set; }
    }
}
