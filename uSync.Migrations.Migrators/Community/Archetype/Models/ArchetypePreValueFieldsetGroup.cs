using Newtonsoft.Json;

namespace uSync.Migrations.Migrators.Community.Archetype.Models;

/// <summary>
/// Model that represents configured groupings of Archetype fieldSets.
/// </summary>
public class ArchetypePreValueFieldSetGroup
{
    [JsonProperty("name")]
    public string? Name { get; set; }
}