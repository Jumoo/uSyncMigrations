using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uSync.Migrations.Handlers;
internal class SyncMigrtionHandlerAttribute : Attribute
{
    public SyncMigrtionHandlerAttribute(string group, int priority, int sourceVersion)
    {
        Group = group;
        Priority = priority;
        SourceVersion = sourceVersion;
    }

    public string Group { get; set; }
    public int Priority { get; set; }
    public int SourceVersion { get; set; }

    public string SourceFolderName { get; set; }
    public string TargetFolderName { get; set; }
}
