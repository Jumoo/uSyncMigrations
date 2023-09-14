using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uSync.Migrations.Migrators.BlockGrid.Config;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.BlockGrid.ViewPropertyMigrators
{
    public class GridViewPropertyBooleanMigrator : IGridSettingsViewMigrator
    {
        public string ViewKey => "Boolean";

        public string NewDataTypeAlias => "True/false";

        public object ConvertContentString(string value)
        {
            return value;
        }
    }
}
