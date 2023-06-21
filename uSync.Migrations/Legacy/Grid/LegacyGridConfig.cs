using Newtonsoft.Json;

using Umbraco.Cms.Core.Configuration.Grid;

using uSync.Migrations.Context;

namespace uSync.Migrations.Legacy.Grid;

/// <summary>
///  for us this replaces IGridConfig everywhere. 
/// </summary>
/// <remarks>
///  
///  editorsByContext - can give us editors for the current 
///  migration
/// </remarks>
public class LegacyGridConfig : ILegacyGridConfig
{
    private IGridConfig _gridConfig;

    public LegacyGridConfig(IGridConfig gridConfig)
    {
        _gridConfig = gridConfig;
    }

    private ILegacyGridEditorsConfig? _legacyEditorsConfig;

    public ILegacyGridEditorsConfig EditorsConfig => _gridConfig.EditorsConfig.ToLegacyEditorsConfig();

    public ILegacyGridEditorsConfig EditorsFromFolder(string folder)
    {
        var config = Path.Combine(folder, "config", "grid.editors.config.js");
        if (!string.IsNullOrEmpty(config) && File.Exists(config))
        {
            _legacyEditorsConfig = LoadLegacy(config);
            if (_legacyEditorsConfig != null) return _legacyEditorsConfig;
        }

        return _gridConfig.EditorsConfig.ToLegacyEditorsConfig();
    }

    public ILegacyGridEditorsConfig EditorsByContext(SyncMigrationContext context)
    {
        // Load the editors based on the context, 
        // sometimes this will be the loaded grid config from a folder
        // inside the migration and other times the sites grid config.
        if (!context.Metadata.SiteFolderIsSite)
        {
            return EditorsFromFolder(context.Metadata.SiteFolder);
        }

        return _gridConfig.EditorsConfig.ToLegacyEditorsConfig();
    }

    private ILegacyGridEditorsConfig? LoadLegacy(string filename)
    {
        try
        {
            if (_legacyEditorsConfig != null) return _legacyEditorsConfig;

            var config = File.ReadAllText(filename);

            var elements = JsonConvert.DeserializeObject<List<LegacyGridEditorConfig>>(config);
            if (elements != null)
            {
                return new LegacyGridEditorsConfig
                {
                    Editors = elements.ToList<ILegacyGridEditorConfig>()
                };
            }

            return default;
        }
        catch(Exception ex)
        {
            // something went wrong parsing the config file.
            throw new Exception($"Error parsing the grid.editors.config.js file {ex.Message}", ex);
        }
    }
}