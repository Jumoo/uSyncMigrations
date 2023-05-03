using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;
using uSync.Migrations.Context;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.Tags)]
public class TagMigrator : SyncPropertyMigratorBase
{
    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var config = new TagConfiguration
        {
            Delimiter = '\u0000'
        };

        if (dataTypeProperty.PreValues == null) return config;


        foreach (var preValue in dataTypeProperty.PreValues)
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
