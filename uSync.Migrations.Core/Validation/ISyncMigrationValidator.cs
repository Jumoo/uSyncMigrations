using Umbraco.Cms.Core.Composing;

using uSync.Migrations.Core.Context;
using uSync.Migrations.Core.Models;

namespace uSync.Migrations.Core.Validation;

public interface ISyncMigrationValidator : IDiscoverable
{
    IEnumerable<MigrationMessage> Validate(SyncValidationContext validationContext);
}
