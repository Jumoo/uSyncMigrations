using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uSync.Migrations.Models;
public class SyncDataTypeInfo
{
    public string Name { get; set; }
    public string EditorAlias { get; set; }
    public string DatabaseType { get; set; }
    public IList<PreValue> PreValues { get; set; }
}
