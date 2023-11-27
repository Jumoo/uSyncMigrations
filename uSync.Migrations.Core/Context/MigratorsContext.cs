using uSync.BackOffice;
using uSync.Migrations.Core.Migrators;

namespace uSync.Migrations.Core.Context;

/// <summary>
///  context for all the loaded migrators we have. 
/// </summary>
/// <remarks>
///  this is used to co-ordinate what migrators we use for what, 
/// </remarks>
public class MigratorsContext
{
    #region migrators 

    /// <summary>
    ///  migrators by editor alias, the most common way to get a migrator. 
    /// </summary>
    private Dictionary<string, ISyncPropertyMigrator> _migrators { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///  get a migrator for a given editor alias, 
    /// </summary>
    /// <param name="editorAlias"></param>
    /// <returns></returns>
    public ISyncPropertyMigrator? TryGetMigrator(string? editorAlias)
    => string.IsNullOrEmpty(editorAlias)
        ? null
        : _migrators.TryGetValue(editorAlias, out var migrator) == true ? migrator : null;

    /// <summary>
    ///  try get a migrator for a property or editor alias. 
    /// </summary>
    /// <remarks>
    ///  if a specific migrator has been set for a property that will be used, 
    ///  but if that fails you get the migrator for the editorAlias. 
    /// </remarks>
    public ISyncPropertyMigrator? TryGetMigrator(string? propertyAlias, string? editorAlias)
        => TryGetPropertyAliasMigrator(propertyAlias) ?? TryGetMigrator(editorAlias);

    /// <summary>
    ///  Add a migrator for a given editorAlias
    /// </summary>
    public void AddPropertyMigration(string editorAlias, ISyncPropertyMigrator migrator)
    {
        _migrators.TryAdd(editorAlias, migrator);
    }

    /// <summary>
    ///  get the variant version of a migrator (if there is one)
    /// </summary>
    public ISyncVariationPropertyMigrator? TryGetVariantMigrator(string editorAlias)
    {
        if (_migrators.TryGetValue(editorAlias, out var migrator)
            && migrator is ISyncVariationPropertyMigrator variationPropertyMigrator)
        {
            return variationPropertyMigrator;
        }

        return null;
    }

    /// <summary>
    /// gets the property splitting version of a migrator (if there is one)
    /// </summary>
    public ISyncPropertySplittingMigrator? TryGetPropertySplittingMigrator(string editorAlias)
    {
        if (_migrators.TryGetValue(editorAlias, out var migrator)
            && migrator is ISyncPropertySplittingMigrator propertySplittingMigrator)
        {
            return propertySplittingMigrator;
        }

        return null;
    }

    #endregion

    #region PropertyAlias migrators 

    /// <summary>
    ///  getting migrators by the property alias.
    /// </summary>
    private Dictionary<string, ISyncPropertyMigrator> _propertyMigrators { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///  try and get the migrator for a given property alias. 
    /// </summary>
    private ISyncPropertyMigrator? TryGetPropertyAliasMigrator(string? propertyAlias)
    {
        if (string.IsNullOrEmpty(propertyAlias)) return null;

        // search for the full pattern
        if (_propertyMigrators.TryGetValue(propertyAlias, out var migrator))
            return migrator;

        // if we haven't found - but its a split (contentType_alias) value split the value and look just for the
        // propertyAlias

        if (propertyAlias.IndexOf('_') > 0)
        {
            var propertyEditorAlias = propertyAlias.Substring(propertyAlias.IndexOf('_') + 1);
            return _propertyMigrators.TryGetValue(propertyEditorAlias, out var propertyAliasMigrator) == true
                ? propertyAliasMigrator : null;
        }
        return null;
    }

    /// <summary>
    ///  add a migration for a property by alias. 
    /// </summary>
    /// <param name="propertyAlias"></param>
    /// <param name="migrator"></param>
    public void AddPropertyAliasMigration(string propertyAlias, ISyncPropertyMigrator migrator)
        => _propertyMigrators.TryAdd(propertyAlias, migrator);

    #endregion

    #region Merging migrators 

    /// <summary>
    ///  migrators that merge two or more properties back into a single property.
    /// </summary>
    private Dictionary<string, ISyncPropertyMergingMigrator> _mergingMigrators { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///  add a migrator that will merge properties for a given content type.
    /// </summary>
    public void AddMergingMigrator(string contentType, ISyncPropertyMergingMigrator mergingMigrator)
        => _ = _mergingMigrators.TryAdd(contentType, mergingMigrator);

    /// <summary>
    ///  get any migrators that merge properties together.
    /// </summary>
    public ISyncPropertyMergingMigrator? GetMergingMigrator(string contentType)
        => _mergingMigrators.TryGetValue(contentType, out var migrator) == true ? migrator : null;

    #endregion

    #region Migrator data 

    /// <summary>
    /// A cache of dictionary items that can be used if you need to store/retrieve custom data between 
    /// config and content mappings
    /// </summary>
    private Dictionary<string, Dictionary<string, object>> _migratorCache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///  add a dictionary of custom values for this datatype alias.
    /// </summary>
    /// <remarks>
    ///  It is the migrators responsibility to make sure this custom set of values is unique and 
    ///  does not clash. recommendation is to use the datatype's alias. 
    /// </remarks>
    public void AddCustomValues(string alias, Dictionary<string, object> values)
        => _ = _migratorCache.TryAdd(alias, values);

    /// <summary>
    ///  retrieve a dictionary of custom values.
    /// </summary>
    /// <param name="alias"></param>
    /// <returns></returns>
    public Dictionary<string, object> GetCustomValues(string alias)
        => _migratorCache.TryGetValue(alias, out Dictionary<string, object>? values)
            ? values : new Dictionary<string, object>();

    #endregion
}
