using Umbraco.Extensions;

using uSync.Migrations.Composing;
using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Context;
using uSync.Migrations.Models;

namespace uSync.Migrations.Validation;

internal class PreferredMigratorValidator : ISyncMigrationValidator
{
    private readonly SyncPropertyMigratorCollection _migrators;

    public PreferredMigratorValidator(SyncPropertyMigratorCollection migrators)
    {
        _migrators = migrators;
    }

    public IEnumerable<MigrationMessage> Validate(SyncValidationContext validationContext)
    {
        var results = new List<MigrationMessage>();

        // we only do this for v7
        if (validationContext.Metadata.SourceVersion != 7) return results;

        var preferredList = _migrators.GetPreferredMigratorList(validationContext.Options.PreferredMigrators);
        foreach(var missing in preferredList.Where(x => x.Migrator == null))
        {
            results.Add(new MigrationMessage("Migrator", "Missing", MigrationMessageType.Error)
            {
                Message = $"There is no migrator assigned for type {missing.EditorAlias}"
            });
        }

        var duplicates = preferredList
            .DistinctBy(x => $"{x.EditorAlias}_{x.Migrator}")
            .GroupBy(x => x.EditorAlias).Where(x => x.Count() > 1);

        if (duplicates.Any())
        {

            foreach (var duplicate in duplicates) {

                results.Add(new MigrationMessage("Migrator", "Duplicate", MigrationMessageType.Error)
                {
                    Message = $"there are two or more migrators configured for " +
                        $"{duplicate.FirstOrDefault()?.EditorAlias ?? ""} " +
                        $"[{string.Join(",", duplicate.Select(x => x.Migrator.GetType().Name))}]"
                });
            }
        }
        else
        {
        }

        if (results.Count == 0)
        {
            return new MigrationMessage("Migrators", "Migrators OK", MigrationMessageType.Success)
                .AsEnumerableOfOne();
        }
        else
        {
            return results;
        }
    }
}
