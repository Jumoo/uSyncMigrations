using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Archetype.Models
{
    /// <summary>
    /// Model that represents an entire Archetype.
    /// </summary>
    [JsonObject]
    public class ArchetypeModel : IEnumerable<ArchetypeFieldsetModel>
    {
        [JsonProperty("fieldsets")]
        public IEnumerable<ArchetypeFieldsetModel> Fieldsets { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchetypeModel"/> class.
        /// </summary>
        public ArchetypeModel()
        {
            Fieldsets = new List<ArchetypeFieldsetModel>();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection of fieldsets.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<ArchetypeFieldsetModel> GetEnumerator()
        {
            return this.Fieldsets.GetEnumerator();
        }

        //possibly obsolete?
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
