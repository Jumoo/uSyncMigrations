using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

public class TagMigrator : SyncPropertyMigratorBase
{
    public override string[] Editors => new[] { UmbConstants.PropertyEditors.Aliases.Tags };

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues, SyncMigrationContext context)
    {
        var config = new TagConfiguration
        {
            Delimiter = '\u0000'
        };

        foreach (var preValue in preValues)
        {
            switch (preValue.Alias)
            {
                case "group":
                    config.Group = preValue.Value;
                    break;

                case "storageType":
                    config.StorageType = preValue.Value.InvariantEquals("csv") == true
                        ? TagsStorageType.Csv
                        : TagsStorageType.Json;
                    break;
            }
        }

        return config;
    }
}
