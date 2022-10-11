
// Converts MediaPicker2 to a v10 MediaPicker (not 3)
// TODO: We really want a way for people to be able to pick 
// between converts (e.g go to MediaPicker or MediaPicker3)

//using Umbraco.Cms.Core.PropertyEditors;

//using uSync.Migrations.Extensions;
//using uSync.Migrations.Models;

//namespace uSync.Migrations.Migrators.DataTypes;
//
//internal class MediaPickerMigrator : SyncMigratorBase
//{
//    public override string[] Editors => new[]
//    {
//        "Umbraco.MediaPicker2"
//    };

//    public override string GetEditorAlias(string editorAlias, string dabaseType)
//        => "Umbraco.MediaPicker";

//    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
//    {
//        var config = new MediaPickerConfiguration();

//        var mappings = new Dictionary<string, string>
//        {
//            { "multiPicker", nameof(config.Multiple) },
//            { "disableFolderSelect", nameof(config.DisableFolderSelect) },
//            { "startNodeId", nameof(config.StartNodeId) },
//            { "onlyImages", nameof(config.OnlyImages) }
//        };

//        return config.MapPreValues(preValues, mappings);
//    }
//}
