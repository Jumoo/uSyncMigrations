using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace uSync.Migrations.Legacy.Grid;

/// <summary>
///  IGridEditorConfig - ready should that interface be removed 
/// </summary>
[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public interface ILegacyGridEditorConfig 
{
    string? Name { get; }

    string? NameTemplate { get; }

    string? Alias { get; }

    string? View { get; }

    string? Render { get; }

    string? Icon { get; }

    IDictionary<string, object> Config { get; }
}

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class LegacyGridEditorConfig : ILegacyGridEditorConfig
{
    public string? Name { get; set; }

    public string? NameTemplate { get; set; }

    public string? Alias { get; set; }

    public string? View { get; set; }

    public string? Render {get;set;}

    public string? Icon { get; set; }

    public IDictionary<string, object> Config
    { get; set; } = new Dictionary<string, object>();
}