using System.Collections.Specialized;

using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

internal class ContentPickerMigration : SyncMigratorBase
{
    public override string[] Editors => new[]
    {
        "Umbraco.ContentPickerAlias",
        "Umbraco.ContentPicker2"
    };

    public override string GetEditorAlias(string editorAlias, string dabaseType)
        => "Umbraco.ContentPicker";

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
    {
        var config = new ContentPickerConfiguration();

        var mappings = new Dictionary<string, string>
        {
            { "showOpenButton", nameof(config.ShowOpenButton) },
            { "startNodeId", nameof(config.StartNodeId) }
        };

        return config.MapPreValues(preValues, mappings);
    }
}

internal class MultiNodeTreePicker : SyncMigratorBase
{
    public override string[] Editors => new[]
    {
        "Umbraco.MultiNodeTreePicker2"
    };

    public override string GetEditorAlias(string editorAlias, string dabaseType)
        => "Umbraco.MultiNodeTreePicker";

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
    {
        var config = new MultiNodePickerConfiguration();
        var mappings = new Dictionary<string, string>
        {
            { "ignoreUserStartNodes", nameof(config.IgnoreUserStartNodes) },
            { "startNode", nameof(config.TreeSource) },
            { "filter", nameof(config.Filter) },
            { "minNumber", nameof(config.MinNumber) },
            { "maxNumber", nameof(config.MaxNumber) },
            { "showOpenButton", nameof(config.ShowOpen) }
        };

        return config.MapPreValues(preValues, mappings);
    }
}
