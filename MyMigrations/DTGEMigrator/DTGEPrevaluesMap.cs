using Newtonsoft.Json;



using uSync.Migrations.Context;

namespace MyMigrations.DTGEMigrator;


/// <summary>
///  for us this replaces IGridConfig everywhere. 
/// </summary>
/// <remarks>
///  
///  editorsByContext - can give us editors for the current 
///  migration
/// </remarks>
public class DTGEPrevaluesMap
{
    private IDictionary<int, string> _prevaluesMap;

    public DTGEPrevaluesMap(SyncMigrationContext context)
    {
        _prevaluesMap = EditorsFromFolder(context.Metadata.SiteFolder);
    }

    public bool HasValue(int preValueId) => _prevaluesMap?.ContainsKey(preValueId) ?? false;
    public string GetValue(int preValueId) => _prevaluesMap?[preValueId] ?? "";

    public IDictionary<int, string> EditorsFromFolder(string folder)
    {
        var config = Path.Combine(folder, "config", "dtgePreValues.config.json");
        if (!string.IsNullOrEmpty(config) && File.Exists(config))
        {
            return LoadValues(config);
        }

        return new Dictionary<int, string>();
    }
    

    private IDictionary<int, string> LoadValues(string filename)
    {
        try
        {
            
            var config = File.ReadAllText(filename);

            var elements = JsonConvert.DeserializeObject<List<PreValueItem>>(config);
            if (elements != null)
            {
                return elements
                    .Where(x => x.Value != null)
                    .ToDictionary(x => x.Id, x => x.Value!);
            }

            return new Dictionary<int, string>();
        }
        catch(Exception ex)
        {
            // something went wrong parsing the config file.
            throw new Exception($"Error parsing the dtgePreValues.config.json file {ex.Message}", ex);
        }
    }

    private class PreValueItem
    {
        public int Id { get; set; }
        public string? Value { get; set; }
    }
}