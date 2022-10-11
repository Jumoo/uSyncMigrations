using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using uSync.Migrations.Models;

using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Extensions;

namespace uSync.Migrations.Migrators;
internal class MultipleTextStringMigrator : SyncMigratorBase
{
    public override string[] Editors => new[]
    {
        "Umbraco.MultipleTextstring"
    };

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
    {
        var config = new MultipleTextStringConfiguration();
        return config.MapPreValues(preValues);
    }
}
