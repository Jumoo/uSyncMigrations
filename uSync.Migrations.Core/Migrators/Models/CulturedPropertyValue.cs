namespace uSync.Migrations.Core.Migrators.Models;


/// <summary>
///  How a single value can be split into multiple 'variants' 
/// </summary>
/// <remarks>
///  This is a direct representation of how Vorto does it. 
///  
///  if there is anything else that does this ??? then it 
///  needs to return something in this format and the 
///  content migrator will do the rest.
/// </remarks>
public class CulturedPropertyValue
{
    /// <summary>
    ///  the GUID of the datatype that can take these values.
    /// </summary>
    public Guid DtdGuid { get; set; }

    /// <summary>
    ///  values organised by culture, 
    /// </summary>
    public Dictionary<string, string> Values { get; set; } = new();
}