using System.Diagnostics.CodeAnalysis;

﻿using Umbraco.Extensions;
using uSync.Migrations.Core.Configuration.Models;
using uSync.Migrations.Core.Models;
using uSync.Migrations.Core.Plans.Models;

namespace uSync.Migrations.Core.Context;

/// <summary>
///  migration context for anything dealing with ContentTypes
/// </summary>
public class ContentTypeMigrationContext
{
    private Dictionary<Guid, string> _contentTypeAliases { get; set; } = new();
    private Dictionary<string, Guid> _contentTypeKeys { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, HashSet<string>> _contentTypeCompositions { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, EditorAliasInfo> _propertyTypes { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    private HashSet<string> _ignoredProperties = new(StringComparer.OrdinalIgnoreCase);

    private Dictionary<string, NewContentTypeInfo> _newDocTypes
            = new Dictionary<string, NewContentTypeInfo>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///  allows you to map property aliases in a content type to the specific datatype
    /// </summary>
    private Dictionary<string, string> _dataTypeAliases = new(StringComparer.OrdinalIgnoreCase);

    // tabs that are to be changed
    private List<TabOptions> _changedTabs { get; set; } = new List<TabOptions>();

    /// <summary>
    ///  list of content types that need to be set as element types. 
    /// </summary>
    private HashSet<Guid> _elementContentTypes = new HashSet<Guid>();

    private Dictionary<string, string> _replacementAliases =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///  Add a content type key to the context.
    /// </summary>
    /// <param name="contentTypeAlias">Alias for the content type</param>
    /// <param name="contentTypeKey">GUID key value</param>
    public void AddAliasAndKey(string? contentTypeAlias, Guid? contentTypeKey)
    {
        if (string.IsNullOrWhiteSpace(contentTypeAlias) is true || contentTypeKey.HasValue is false)
            return;

        _ = _contentTypeAliases.TryAdd(contentTypeKey.Value, contentTypeAlias);
        _ = _contentTypeKeys.TryAdd(contentTypeAlias, contentTypeKey.Value);
    }

    /// <summary>
    ///  get the alias of a content type by providing the key value 
    /// </summary>
    public bool TryGetAliasByKey(Guid contentTypeKey, [MaybeNullWhen(false)]out string alias)
        => _contentTypeAliases.TryGetValue(contentTypeKey, out alias);

    /// <summary>
    ///  get the key for a given content type alias from the context.
    /// </summary>
    public bool TryGetKeyByAlias(string contentTypeAlias, [MaybeNullWhen(false)]out Guid key)
        => _contentTypeKeys.TryGetValue(contentTypeAlias, out key);

    /// <summary>
    ///  return all the content aliases we have loaded in the context.
    /// </summary>
    /// <returns>array of aliases</returns>
    public string[] GetAllAliases()
        => _contentTypeAliases?.Values?.ToArray() ?? Array.Empty<string>();

    /// <summary>
    ///  add content type compositions to the context
    /// </summary>
    public void AddCompositions(string? contentTypeAlias, IEnumerable<string>? compositionAliases)
    {
        _ = string.IsNullOrWhiteSpace(contentTypeAlias) == false &&
            compositionAliases?.Any() == true &&
            _contentTypeCompositions.TryAdd(contentTypeAlias, compositionAliases.ToHashSet());
    }

    /// <summary>
    ///  gets the list of aliases for compositions of a content type
    /// </summary>
    /// <param name="contentTypeAlias">Alias of the content type</param>
    /// <param name="compositionAliases">Enumerable of alises for the compositions</param>
    /// <returns>true if succeeded.</returns>
    public bool TryGetCompositionsByAlias(string? contentTypeAlias, out IEnumerable<string> compositionAliases)
    {
        compositionAliases = Enumerable.Empty<string>();

        if (contentTypeAlias != null && _contentTypeCompositions.TryGetValue(contentTypeAlias, out var compositions))
        {
            compositionAliases = compositions.ToArray();
        }

        return compositionAliases.Any();
    }


    /// <summary>
    ///  Add a editorAlias mapping for a property mapping to the context.
    /// </summary>
    /// <remarks>
    ///  allows you to track when the editor alias of a property changes from original to a new value
    /// </remarks>

    public void AddProperty(string? contentTypeAlias, string? propertyAlias, string? originalAlias, string? newAlias, Guid? dataTypeDefinition = default)
    {
        _ = string.IsNullOrWhiteSpace(contentTypeAlias) == false &&
            string.IsNullOrWhiteSpace(propertyAlias) == false &&
            string.IsNullOrWhiteSpace(originalAlias) == false &&
            string.IsNullOrWhiteSpace(newAlias) == false &&
            _propertyTypes.TryAdd($"{contentTypeAlias}_{propertyAlias}",
            new EditorAliasInfo(originalAlias, newAlias, dataTypeDefinition));
    }

    /// <summary>
    ///  get the migrated editro alias for a property based on the content type it is in.
    /// </summary>
    /// <remarks>
    ///  this has to be done by content type, because when we are in content, we don't know
    ///  about the underling data type. 
    ///  
    ///  so when content types are prepped for migration they add this key pair (AddContentProperty)
    ///  and then when we are in content we can say, what is the underling property for this 
    ///  value based on the content type we know we are in. 
    /// </remarks>
    public bool TryGetEditorAliasByTypeAndProperty(string contentTypeAlias, string propertyAlias, [MaybeNullWhen(false)] out EditorAliasInfo alias)
    {
        if (_propertyTypes.TryGetValue($"{contentTypeAlias}_{propertyAlias}", out alias) is true)
        {
            return true;
        }

        if (TryGetEditorAliasByComposition(contentTypeAlias, propertyAlias, out alias) is true)
        {
            return true;
        }

        return false; 
    }

    /// <summary>
    ///  returns the editor alias from a property within a composition of a content type.
    /// </summary>
    private bool TryGetEditorAliasByComposition(string compositionKey, string propertyAlias, [MaybeNullWhen(false)] out EditorAliasInfo editorAliasInfo)
    {
        if (_contentTypeCompositions?.TryGetValue(compositionKey, out var compositions) == true)
        {
            foreach (var composition in compositions)
            {
                if (_propertyTypes?.TryGetValue($"{composition}_{propertyAlias}", out var alias) == true)
                {
                    editorAliasInfo = alias;
                    return true;
                }

                if (TryGetEditorAliasByComposition(composition, propertyAlias, out editorAliasInfo) == true)
                {
                    return true;
                }
            }
        }

        editorAliasInfo = null;
        return false;
    }

    /// <summary>
    ///  tells us if a content type is an element type
    /// </summary>
    public bool IsElementType(Guid key) => _elementContentTypes.Contains(key);

    /// <summary>
    ///  add an element type to the list of element types.
    /// </summary>
    public void AddElementType(Guid key)
    {
        if (!_elementContentTypes.Contains(key)) _elementContentTypes.Add(key);
    }

    /// <summary>
    ///  add a list of element types by guid.
    /// </summary>
    public void AddElementTypes(IEnumerable<Guid> contentTypeKeys, bool includeCompositions)
    {
        foreach (var contentTypeKey in contentTypeKeys)
        {
            AddElementType(contentTypeKey);

            if (!includeCompositions)
            {
                continue;
            }

            if (TryGetAliasByKey(contentTypeKey, out var contentTypeAlias) is false) { continue; }

            if (TryGetCompositionsByAlias(contentTypeAlias, out var compositionAliases) is false) { continue; }

            foreach (var compositionContentTypeAlias in compositionAliases)
            {
                if (TryGetKeyByAlias(compositionContentTypeAlias, out var key) is true)
                {
                    AddElementType(key);
                }
            }
        }
    }


    /// <summary>
    ///  ignore a property on a specific content type. 
    /// </summary>
    /// <remarks>
    ///  note this is the final content type, will not calculate compositions.
    /// </remarks>
    public void AddIgnoredProperty(string contentType, string alias)
        => _ = _ignoredProperties.Add($"{contentType}_{alias}");

    /// <summary>
    ///  add a property to ignore for all content types.
    /// </summary>
    public void AddIgnoredProperty(string alias)
    => _ = _ignoredProperties.Add($"{alias}");

    /// <summary>
    ///  returns true if a property is to be ignored 
    /// </summary>
    public bool IsIgnoredProperty(string contentType, string alias)
        => _ignoredProperties.Contains($"{contentType}_{alias}")
        || _ignoredProperties.Contains(alias);

    /// <summary>
    ///  add a new content type - will then be processed as part of the 
    ///  migration process.
    /// </summary>
    public void AddNewContentType(NewContentTypeInfo newDocTypeInfo)
    {
        if (!_newDocTypes.ContainsKey(newDocTypeInfo.Alias))
            _newDocTypes.Add(newDocTypeInfo.Alias, newDocTypeInfo);
    }

    /// <summary>
    ///  list of all the new content types to be created. 
    /// </summary>
    /// <returns></returns>
    public IList<NewContentTypeInfo> GetNewContentTypes()
        => _newDocTypes.Values.ToList();


    private Dictionary<string, string> _blockAliases
        = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///  add a block editor to by name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="alias"></param>
    public void AddBlockEditor(string name, string alias)
        => _blockAliases.TryAdd(name, alias);

    /// <summary>
    ///  get the block editor alias from the name.
    /// </summary>
    public bool TryGetBlockEditorAliasByName(string name, [MaybeNullWhen(false)] out string alias)
        => _blockAliases.TryGetValue(name, out alias);

    /// <summary>
    /// Add changed tabs to the context.
    /// </summary>
    public void AddChangedTabs(TabOptions tab)
        => _changedTabs.Add(tab);

    /// <summary>
    ///  list of the changed tab names (renames)
    /// </summary>
    /// <returns></returns>
    public List<TabOptions> GetChangedTabs()
        => _changedTabs;

    /// <summary>
    ///  add a mapping for a content types property to the named datatype.
    /// </summary>
    public void AddDataTypeAlias(string contentTypeAlias, string propertyAlias, string dataTypeAlias)
        => _ = _dataTypeAliases.TryAdd($"{contentTypeAlias}_{propertyAlias}", dataTypeAlias);

    /// <summary>
    ///  get the datatype alias for a property on a content type.
    /// </summary>
    /// <param name="contentTypeAlias"></param>
    /// <param name="propertyAlias"></param>
    /// <returns></returns>
    public bool TryGetDataTypeAlias(string contentTypeAlias, string propertyAlias, [MaybeNullWhen(false)] out string alias)
        => _dataTypeAliases.TryGetValue($"{contentTypeAlias}_{propertyAlias}", out alias);

    /// <summary>
    /// Add a replacement alias for a content type alias
    /// </summary>
    public void AddReplacementAlias(string original, string replacement)
        => _replacementAliases.TryAdd(original, replacement);

    /// <summary>
    ///  get the replacement alias for an property based on the current alias.
    /// </summary>
    /// <param name="alias"></param>
    /// <returns></returns>
    public string GetReplacementAlias(string alias)
        => _replacementAliases.TryGetValue(alias, out var replacement)
            ? replacement : alias;


    public void UpdatePropertyEditorTargets(DataTypeMigrationContext dataTypeContext)
    {
        foreach (var property in _propertyTypes.Values)
        {
            var editorAlias = property.UpdatedEditorAlias;

            var newEditorName = dataTypeContext.GetPropertyEditorReplacementName(editorAlias);

            if (newEditorName.IsNullOrWhiteSpace() == true)
            {
                continue;
            }
            property.UpdatedEditorAlias = newEditorName;
        }
    }
}
