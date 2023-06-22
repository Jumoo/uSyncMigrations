using uSync.Migrations.Configuration.Models;

namespace uSync.Migrations.Context;

/// <summary>
///  context passed between items during validation
/// </summary>
public class SyncValidationContext
{
    public SyncValidationContext(MigrationOptions options,
        Guid migrationId, string sourceFolder, string siteFolder, bool siteIsSite, int version)
    {
        Metadata = new MigrationContextMetadata(migrationId, sourceFolder, siteFolder, siteIsSite, version);
        Options = options;       
    }

    public MigrationOptions Options { get; set; }
    public MigrationContextMetadata Metadata { get; set; }

}
