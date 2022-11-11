namespace uSync.Migrations.Migrators.Models;


/// <summary>
///  How a single value can be split into multiple 'variants' 
/// </summary>
/// <remarks>
///  This is a direct represetation of how Vorto does it. 
///  
///  if there is anything else that does this ??? then it 
///  needs to return something in this format and the 
///  content migrator will do the rest.
/// </remarks>
public class CulturedPropertyValue
{
    public Guid DtdGuid { get; set; }
    public Dictionary<string, string> Values { get; set; } = new();
}