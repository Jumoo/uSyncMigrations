using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Umbraco.Extensions;

namespace uSync.Migrations.Migrators.Content;
internal class DropdownListValueMigrator : ISyncContentPropertyMigrator
{
    public string[] Editors => new[]
    {
        "Umbraco.DropDown.Flexible",
        "Umbraco.DropDown"
    };

    public string GetMigratedValue(string editorAlias, string value)
       => JsonConvert.SerializeObject(value.ToDelimitedList());
}
