using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;
internal class NoEdittToLabelMigrator : SyncMigratorBase
{
    public override string[] Editors => new[] { "Umbraco.NoEdit" };

    public override string GetEditorAlias(string editorAlias, string dabaseType)
        => "Umbraco.Label";

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
    {
        var config = new LabelConfiguration();
        config.ValueType = preValues.GetPreValueOrDefault("umbracoDataValueType", "STRING");
        return config;
    }
}
