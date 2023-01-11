// using Newtonsoft.Json;
// using Newtonsoft.Json.Linq;
// using Umbraco.Cms.Core.Configuration.Grid;
// using Umbraco.Cms.Core.Models;
// using uSync.Migrations.Migrators;
// using uSync.Migrations.Migrators.Models;
// using uSync.Migrations.Models;
//
// namespace uSyncMigrationSite;
//
// [SyncDefaultMigrator]
// [SyncMigrator("Umbraco.Grid", ConfigurationType = typeof(Umbraco.Cms.Core.PropertyEditors.GridConfiguration))]
// public class DtgeGridMigrator : GridMigrator
// {
//     private readonly IGridConfig _gridConfig;
//     private readonly ILogger<DtgeGridMigrator> _logger;
//
//     public DtgeGridMigrator(IGridConfig gridGridConfig, ILogger<DtgeGridMigrator> logger)
//     {
//         _gridConfig = gridGridConfig;
//         _logger = logger;
//     }
//
//     public override object GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
//     {
//         foreach (var editor in _gridConfig.EditorsConfig.Editors)
//         {
//             if (editor?.Config.TryGetValue("allowedDocTypes", out var allowedDocTypes) != true)
//             {
//                 continue;
//             }
//
//             if (allowedDocTypes is JArray docTypes)
//             {
//                 foreach (var docType in docTypes.Values<string>().Where(x => x != null))
//                 {
//                     if (docType.StartsWith("^") && docType.EndsWith("$"))
//                     {
//                         var contentTypeAlias = docType.TrimStart('^').TrimEnd('$');
//
//                         var contentTypeKey = context.GetContentTypeKey(contentTypeAlias);
//                         context.AddElementType(contentTypeKey);
//
//                         if (context.TryGetContentTypeCompositions(contentTypeAlias, out var compositionAliases) &&
//                             compositionAliases != null)
//                         {
//                             foreach (var compositionAlias in compositionAliases)
//                             {
//                                 var compositionTypeKey = context.GetContentTypeKey(compositionAlias);
//                                 context.AddElementType(compositionTypeKey);
//                             }
//                         }
//                     }
//                     else
//                     {
//                         // TODO : log missing matches
//                     }
//                 }
//             }
//         }
//
//         return base.GetConfigValues(dataTypeProperty, context);
//     }
//
//     public override string GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
//     {
//         var value = base.GetContentValue(contentProperty, context);
//
//         if (string.IsNullOrWhiteSpace(value))
//         {
//             return value;
//         }
//
//         var grid = JsonConvert.DeserializeObject<GridValue>(value);
//
//         if (grid == null)
//         {
//             return value;
//         }
//
//         foreach (var section in grid.Sections)
//         foreach (var row in section.Rows)
//         foreach (var area in row.Areas)
//         foreach (var control in area.Controls)
//         {
//             MigrateDtgeValue(context, control);
//         }
//
//         return JsonConvert.SerializeObject(grid);
//     }
//
//     private void MigrateDtgeValue(SyncMigrationContext context, GridValue.GridControl control)
//     {
//         if (control.Value?.HasValues != true)
//         {
//             return;
//         }
//         
//         var dtgeContentTypeAlias = control.Value.Value<string>("dtgeContentTypeAlias");
//
//         if (string.IsNullOrWhiteSpace(dtgeContentTypeAlias))
//         {
//             return;
//         }
//
//         var dtgeValue = control.Value.Value<JObject>("value");
//         if (dtgeValue == null)
//         {
//             return;
//         }
//
//         foreach (var (alias, value) in dtgeValue)
//         {
//             var editor = context.GetEditorAlias(dtgeContentTypeAlias, alias);
//
//             if (editor == null)
//             {
//                 continue;
//             }
//
//             var migrator = context.TryGetMigrator(editor.OriginalEditorAlias);
//
//             if (migrator == null)
//             {
//                 continue;
//             }
//
//             var property = new SyncMigrationContentProperty(editor.OriginalEditorAlias, value.ToString());
//
//             var contentValue = migrator.GetContentValue(property, context);
//
//             dtgeValue[alias] = contentValue;
//         }
//     }
// }