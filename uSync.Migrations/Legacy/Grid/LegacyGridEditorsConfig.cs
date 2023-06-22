using Newtonsoft.Json;

namespace uSync.Migrations.Legacy.Grid;

internal class LegacyGridEditorsConfig : ILegacyGridEditorsConfig
{
    public List<ILegacyGridEditorConfig> Editors { get; set; }
            = new List<ILegacyGridEditorConfig>();

    /// <summary>
    ///  load the config editors from disk.
    /// </summary>
    /// <param name="filepath"></param>
    public void LoadEditors(string filepath)
    {
        if (!File.Exists(filepath)) return;

        var contents = File.ReadAllText(filepath);
        if (string.IsNullOrWhiteSpace(contents)) return;

        Editors.AddRange(JsonConvert.DeserializeObject<List<ILegacyGridEditorConfig>>(contents) 
            ?? new List<ILegacyGridEditorConfig>());

        Editors = Editors.DistinctBy(x => x.Alias).ToList();
    }
}