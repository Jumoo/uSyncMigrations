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
    private Dictionary<Guid, DataTypeInfo> _dataTypeDefinitions { get; set; } = new();

    /// <summary>
    ///  when we replace an datatype with something else .
    /// </summary>
    private Dictionary<Guid, Guid> _dataTypeReplacements { get; set; } = new();

    /// <summary>
    ///  datatypes that vary by something (e.g culture)
    /// </summary>
    private Dictionary<Guid, string> _dataTypeVariations { get; set; } = new();

    private Dictionary<string, string> _dataTypePropertyEditorsReplacements = new();


    /// <summary>
    ///  contains a lookup from defition (guid) to alias, so we can pass that along. 
    /// </summary>
    private Dictionary<Guid, string> _dataTypeAliases { get; set; } = new();

    private Dictionary<string, NewDataTypeInfo> _newDataTypes
            = new Dictionary<string, NewDataTypeInfo>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///  add a datatype alias to the lookup
    /// </summary>
    /// <param name="dtd"></param>
    /// <param name="alias"></param>
    public void AddAlias(Guid dtd, string alias)
        => _ = _dataTypeAliases.TryAdd(dtd, alias);

    /// <summary>
    ///  get the alias based on the DTD value (which we have in contenttype).
    /// </summary>
    /// <param name="dtd"></param>
    /// <returns></returns>
    public string GetAlias(Guid dtd)
        => _dataTypeAliases?.TryGetValue(dtd, out var alias) == true
            ? alias : string.Empty;

    /// <summary>
    ///  add a datatypedefinion (aka datatype key) to the context.
    /// </summary>
    public void AddDefinition(Guid dtd, DataTypeInfo def)
        => _ = _dataTypeDefinitions.TryAdd(dtd, def);

    /// <summary>
    ///  get a datatype definiton from the context.
    /// </summary>
    public DataTypeInfo? GetByDefinition(Guid guid)
        => _dataTypeDefinitions?.TryGetValue(guid, out var def) == true
            ? def
            : null;

    /// <summary>
    ///  add the key that replaces a datatype to the context.
    /// </summary>
    public void AddReplacement(Guid original, Guid replacement)
        => _ = _dataTypeReplacements.TryAdd(original, replacement);

    /// <summary>
    ///  get any replacement key values for a given datatype key
    /// </summary>
    public Guid GetReplacement(Guid original)
        => _dataTypeReplacements?.TryGetValue(original, out var replacement) == true
            ? replacement
            : original;

    /// <summary>
    ///  add a variation (e.g culture, segment or nothing) value for a datatype to the context.
    /// </summary>
    public void AddVariation(Guid guid, string variation)
        => _ = _dataTypeVariations?.TryAdd(guid, variation);

    /// <summary>
    ///  retrieve the variation that a datatype will ask a doctype property to perform.
    /// </summary>
    public string GetVariation(Guid guid, string defaultValue)
        => _dataTypeVariations?.TryGetValue(guid, out var variation) == true
            ? variation : defaultValue;

    /// <summary>
    ///  add a new data type - will then be processed as part of the 
    ///  migration process.
    /// </summary>
    public void AddNewDataType(NewDataTypeInfo newDataType)
    {
        if (!_newDataTypes.ContainsKey(newDataType.Alias))
            _newDataTypes.Add(newDataType.Alias, newDataType);
    }

    /// <summary>
    ///  list of all the new data types to be created. 
    /// </summary>
    /// <returns></returns>
    public IList<NewDataTypeInfo> GetNewDataTypes()
        => _newDataTypes.Values.ToList();

    /// <summary>
    ///  get new datatype by alias
    /// </summary>
    /// <returns></returns>
    public NewDataTypeInfo? GetNewDataType(string alias)
        => _newDataTypes.Values.FirstOrDefault(dt => dt.Alias == alias);

    /// <summary>
    ///  return the first definition that we find matching the editorAlias
    /// </summary>
    /// <param name="alias"></param>
    /// <returns></returns>
    public Guid? GetFirstDefinition(string alias)
    {
        var dataTypeDefinition = _dataTypeDefinitions?.FirstOrDefault(x => x.Value.EditorAlias == alias);
        return dataTypeDefinition?.Key ?? null;
    }

    public void AddPropertyEditorsReplacementNames(string editorAlias, string newEditorAlias)
    {
        if (editorAlias != newEditorAlias)
            _dataTypePropertyEditorsReplacements.TryAdd(editorAlias, newEditorAlias);
    }

    public string? GetPropertyEditorReplacementName(string editorAlias)
    {
        if (_dataTypePropertyEditorsReplacements.TryGetValue(editorAlias, out var value))
            return value;

        return null;
    }
}
