using Newtonsoft.Json;
using uSync.Migrations.Context;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators.Community.SkttlHtmlEditor;

[SyncMigrator("skttl.HtmlEditor")]
[SyncMigratorVersion(7, 8)]
public class SkttlHtmlEditorToContentmentCodeEditorMigrator : SyncPropertyMigratorBase
{
    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
  => "Umbraco.Community.Contentment.CodeEditor";
    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var config = JsonConvert.DeserializeObject("{\r\n  \"mode\": \"razor\",\r\n  \"theme\": \"chrome\",\r\n  \"fontSize\": \"small\",\r\n  \"useWrapMode\": \"0\",\r\n  \"minLines\": 12,\r\n  \"maxLines\": 30\r\n}");
        return config;

    }
}
