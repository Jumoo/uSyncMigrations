using uSync.Migrations.Core.Models;

namespace uSync.Migrations.Core.Context;

/// <summary>
///  migrations for data types. 
/// </summary>
public class DataTypeMigrationContext
{
    /// <summary>
    ///  list of keys to editor aliases used to lookup datatypes in content types !
    /// </summary>
    public Dictionary<Guid, DataTypeInfo> DataTypeDefinitions { get; } = new();

    /// <summary>
    ///  when we replace an datatype with something else .
    /// </summary>
    public Dictionary<Guid, Guid> DataTypeReplacements { get; } = new();

    /// <summary>
    ///  datatypes that vary by something (e.g culture)
    /// </summary>
    public Dictionary<Guid, string> DataTypeVariations { get; } = new();


    /// <summary>
    ///  contains a lookup from defition (guid) to alias, so we can pass that along. 
    /// </summary>
    public Dictionary<Guid, string> DataTypeAliases { get; } = new();

    /// <summary>
    ///  add a datatype alias to the lookup
    /// </summary>
    /// <param name="dtd"></param>
    /// <param name="alias"></param>
    public void AddAlias(Guid dtd, string alias)
        => _ = DataTypeAliases.TryAdd(dtd, alias);

    /// <summary>
    ///  get the alias based on the DTD value (which we have in contenttype).
    /// </summary>
    /// <param name="dtd"></param>
    /// <returns></returns>
    public string GetAlias(Guid dtd)
        => DataTypeAliases?.TryGetValue(dtd, out var alias) == true
            ? alias : string.Empty;

    /// <summary>
    ///  add a datatypedefinion (aka datatype key) to the context.
    /// </summary>
    public void AddDefinition(Guid dtd, DataTypeInfo def)
        => _ = DataTypeDefinitions.TryAdd(dtd, def);

    /// <summary>
    ///  get a datatype definiton from the context.
    /// </summary>
    public DataTypeInfo? GetByDefinition(Guid guid)
        => DataTypeDefinitions?.TryGetValue(guid, out var def) == true
            ? def
            : null;

    /// <summary>
    ///  add the key that replaces a datatype to the context.
    /// </summary>
    public void AddReplacement(Guid original, Guid replacement)
        => _ = DataTypeReplacements.TryAdd(original, replacement);

    /// <summary>
    ///  get any replacement key values for a given datatype key
    /// </summary>
    public Guid GetReplacement(Guid original)
        => DataTypeReplacements?.TryGetValue(original, out var replacement) == true
            ? replacement
            : original;

    /// <summary>
    ///  add a variation (e.g culture, segment or nothing) value for a datatype to the context.
    /// </summary>
    public void AddVariation(Guid guid, string variation)
        => _ = DataTypeVariations?.TryAdd(guid, variation);

    /// <summary>
    ///  retrieve the variation that a datatype will ask a doctype property to perform.
    /// </summary>
    public string GetVariation(Guid guid, string defaultValue)
        => DataTypeVariations?.TryGetValue(guid, out var variation) == true
            ? variation : defaultValue;

    /// <summary>
    ///  return the first definition that we find matching the editorAlias
    /// </summary>
    /// <param name="alias"></param>
    /// <returns></returns>
    public Guid? GetFirstDefinition(string alias)
    {
        var dataTypeDefinition = DataTypeDefinitions?.FirstOrDefault(x => x.Value.EditorAlias == alias);
        return dataTypeDefinition?.Key ?? null;
    }

}
