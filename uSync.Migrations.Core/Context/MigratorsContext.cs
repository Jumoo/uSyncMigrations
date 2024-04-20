using System.Diagnostics.CodeAnalysis;

using Org.BouncyCastle.Bcpg.Sig;

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

    public bool TryGetMigrator(string? editorAlias, [MaybeNullWhen(false)] out ISyncPropertyMigrator migrator)
    {
        if (string.IsNullOrEmpty(editorAlias))
        {
            migrator = null;
            return false;
        }

        return _migrators.TryGetValue(editorAlias, out migrator);
    }

    /// <summary>
    ///  try get a migrator for a property or editor alias. 
    /// </summary>
    /// <remarks>
    ///  if a specific migrator has been set for a property that will be used, 
    ///  but if that fails you get the migrator for the editorAlias. 
    /// </remarks>
    public bool TryGetMigrator(string propertyAlias, string editorAlias, [MaybeNullWhen(false)] out ISyncPropertyMigrator propertyMigrator)
    {       
        if (TryGetPropertyAliasMigrator(propertyAlias, out propertyMigrator) is true)
            return true;

        return TryGetMigrator(editorAlias, out propertyMigrator);
    }

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
    public bool TryGetPropertySplittingMigrator(string editorAlias, [MaybeNullWhen(false)] out ISyncPropertySplittingMigrator migrator)
    {
        migrator = null;
        if (_migrators.TryGetValue(editorAlias, out var migratorInternal)
            && migratorInternal is ISyncPropertySplittingMigrator splittingMigrator)
        {
            migrator = splittingMigrator;
            return true;
        }

        return false;
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
    private bool TryGetPropertyAliasMigrator(string propertyAlias, [MaybeNullWhen(false)] out ISyncPropertyMigrator propertyMigrator)
    {
        propertyMigrator = null;
        if (string.IsNullOrEmpty(propertyAlias)) { return false; }

        if (_propertyMigrators.TryGetValue(propertyAlias, out propertyMigrator))
            return true;

        // try without the split.
        if (propertyAlias.IndexOf('_') > 0)
        {
            var propertyEditorAlias = propertyAlias.Substring(propertyAlias.IndexOf('_') + 1);
            return _propertyMigrators.TryGetValue(propertyEditorAlias, out var propertyAliasMigrator);
        }

        return false;
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
    public bool TryGetMergingMigrator(string contentTypeAlias, [MaybeNullWhen(false)] out ISyncPropertyMergingMigrator migrator)
        => _mergingMigrators.TryGetValue(contentTypeAlias, out migrator);

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
