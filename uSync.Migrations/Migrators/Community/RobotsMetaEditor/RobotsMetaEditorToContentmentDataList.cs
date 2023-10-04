using Newtonsoft.Json;
using uSync.Migrations.Context;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators.Community.RobotsMetaEditor;

[SyncMigrator("robotsMetaEditor")]
[SyncMigratorVersion(7, 8)]
public class RobotsMetaEditorToContentmentDataList : SyncPropertyMigratorBase
{
    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => "Umbraco.Community.Contentment.DataList";

    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty,
        SyncMigrationContext context)
    {
        var config = JsonConvert.DeserializeObject("{\r\n  \"dataSource\": [\r\n    {\r\n      \"key\": \"Umbraco.Community.Contentment.DataEditors.UserDefinedDataListSource, Umbraco.Community.Contentment\",\r\n      \"value\": {\r\n        \"items\": [\r\n          {\r\n            \"icon\": \"icon-stop color-black\",\r\n            \"name\": \"Index this, and follow links\",\r\n            \"value\": \"index,follow,noodp\",\r\n            \"description\": \"Will allow indexing of this page, and allow the search engine to follow links\"\r\n          },\r\n          {\r\n            \"icon\": \"icon-stop\",\r\n            \"name\": \"Dont index this, but follow links\",\r\n            \"value\": \"noindex,follow,noodp\",\r\n            \"description\": \"Will disallow indexing of this page, but will allow the search engine to follow links\"\r\n          },\r\n          {\r\n            \"icon\": \"icon-stop\",\r\n            \"name\": \"Index none\",\r\n            \"value\": \"noindex,nofollow,noodp\",\r\n            \"description\": \"Will disallow indexing of this page, and disallow it to follow links\"\r\n          }\r\n        ]\r\n      }\r\n    }\r\n  ],\r\n  \"listEditor\": [\r\n    {\r\n      \"key\": \"Umbraco.Community.Contentment.DataEditors.RadioButtonListDataListEditor, Umbraco.Community.Contentment\",\r\n      \"value\": {\r\n        \"showDescriptions\": \"1\",\r\n        \"showIcons\": \"0\",\r\n        \"allowClear\": \"0\"\r\n      }\r\n    }\r\n  ]\r\n}");
        return config;
    }
}
