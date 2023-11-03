using Umbraco.Cms.Core.Composing;

namespace uSync.Migrations.Migrators.Validation;

public interface ISyncMigrationValidator : IDiscoverable
{
    IEnumerable<MigrationMessage> Validate(SyncValidationContext validationContext);
}
