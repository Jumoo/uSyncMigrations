using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;
public class TagMigrator : SyncMigratorBase
{
    public override string[] Editors => new[] { "Umbraco.Tags" };

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
    {
        var config = new TagConfiguration();
        config.Delimiter = '\u0000';

        foreach (var preValue in preValues)
        {
            switch (preValue.Alias)
            {
                case "group":
                    config.Group = preValue.Value;
                    break;
                case "storageType":
                    config.StorageType =
                        preValue.Value.InvariantEquals("csv") ? TagsStorageType.Csv : TagsStorageType.Json;
                    break;
            }
        }

        return config;

    }
}
