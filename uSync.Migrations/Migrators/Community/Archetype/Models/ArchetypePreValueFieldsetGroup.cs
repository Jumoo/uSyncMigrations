using Newtonsoft.Json;

namespace Archetype.Models
{
    /// <summary>
    /// Model that represents configured groupings of Archetype fieldSets.
    /// </summary>
    public class ArchetypePreValueFieldSetGroup
    {
        [JsonProperty("name")]
        public string? Name { get; set; }
    }
}