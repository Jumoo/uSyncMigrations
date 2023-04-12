using Newtonsoft.Json;

namespace Archetype.Models
{
    /// <summary>
    /// Model that represents a fieldset stored as content JSON.
    /// </summary>
    public class ArchetypeFieldsetModel
    {
        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("disabled")]
        public bool Disabled { get; set; }

        [JsonProperty("properties")]
        public IEnumerable<ArchetypePropertyModel> Properties;

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("releaseDate")]
        public DateTime? ReleaseDate { get; set; }

        [JsonProperty("expireDate")]
        public DateTime? ExpireDate { get; set; }

        [JsonProperty("allowedMemberGroups")]
        public string AllowedMemberGroups { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchetypeFieldsetModel"/> class.
        /// </summary>
        public ArchetypeFieldsetModel()
        {
            Properties = new List<ArchetypePropertyModel>();
        }
    }
}
