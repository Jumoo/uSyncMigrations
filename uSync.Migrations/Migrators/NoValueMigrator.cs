using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;
internal class NoValueMigrator : SyncMigratorBase
{
    public override string[] Editors => new[]
    {
        "Umbraco.MemberPicker2",
        "UmbracoForms.FormPicker"
    };

    private Dictionary<string, string> mappings = new Dictionary<string, string>
    {
        { "Umbraco.MemberPicker2", "Umbraco.MemberPicker" }
    };

    public override string GetEditorAlias(string editorAlias, string dabaseType)
    {
        if (mappings.ContainsKey(editorAlias))
            return mappings[editorAlias];

        return editorAlias;
    }


    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
        => null;
}
