using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace uSync.Migrations.Models;

/// <summary>
///  status of a migration, is persisted to migration.status
/// </summary>
[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class MigrationStatus
{
    public string Icon { get; set; } = "icon-folder";

    public string? Root { get; set; }

    public string? Id { get; set; }

    public int Version { get; set; } = 0;

    public string? Name { get; set; } = "Unknown";
    
    /// <summary>
    ///  where the uSync files are
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    ///  where things from the site are
    /// </summary>
    public string? SiteFolder { get; set; }

    /// <summary>
    ///  where the migrated files are
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    ///  set to true whe the migration has been ran.
    /// </summary>
    /// <remarks>
    /// we could just look for the existance of the migrated folder???
    /// </remarks>
    public bool Migrated { get; set; }

    /// <summary>
    ///  flag for each type of thing to say if its been imported
    /// </summary>
    public Dictionary<string, bool> ImportStatus { get; set; } = new(StringComparer.OrdinalIgnoreCase);


    /// <summary>
    ///  the profile we are going to use for this migration
    /// </summary>
    public string? Plan { get; set; }
}

