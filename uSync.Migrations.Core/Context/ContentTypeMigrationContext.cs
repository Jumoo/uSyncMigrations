﻿using uSync.Migrations.Core.Configuration.Models;
using uSync.Migrations.Core.Models;

namespace uSync.Migrations.Core.Context;

/// <summary>
///  migration context for anything dealing with ContentTypes
/// </summary>
public class ContentTypeMigrationContext
{
    public Dictionary<Guid, string> ContentTypeAliases { get; } = new();
    public Dictionary<string, Guid> ContentTypeKeys { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, HashSet<string>> ContentTypeCompositions { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, EditorAliasInfo> PropertyTypes { get; } = new(StringComparer.OrdinalIgnoreCase);

    public HashSet<string> IgnoredProperties { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, NewContentTypeInfo> NewDocTypes { get; }
            = new Dictionary<string, NewContentTypeInfo>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///  allows you to map property aliases in a content type to the specific datatype
    /// </summary>
    public Dictionary<string, string> DataTypeAliases { get; } = new(StringComparer.OrdinalIgnoreCase);

    // tabs that are to be changed
    private List<TabOptions> _changedTabs { get; set; } = new List<TabOptions>();

    /// <summary>
    ///  list of content types that need to be set as element types. 
    /// </summary>
    public HashSet<Guid> ElementContentTypes { get; } = new HashSet<Guid>();

    public Dictionary<string, string> ReplacementAliases { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string> BlockAliases { get; }
        = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///  Add a content type key to the context.
    /// </summary>
    /// <param name="contentTypeAlias">Alias for the content type</param>
    /// <param name="contentTypeKey">GUID key value</param>
    public void AddAliasAndKey(string? contentTypeAlias, Guid? contentTypeKey)
    {
        _ = string.IsNullOrWhiteSpace(contentTypeAlias) == false &&
            contentTypeKey.HasValue == true &&
            ContentTypeAliases.TryAdd(contentTypeKey.Value, contentTypeAlias) &&
            ContentTypeKeys.TryAdd(contentTypeAlias, contentTypeKey.Value);
    }

    /// <summary>
    ///  get the alias of a content type by providing the key value 
    /// </summary>
    /// <param name="contentTypeKey"></param>
    /// <returns>alias of a content type</returns>
    public string GetAliasByKey(Guid contentTypeKey)
        => ContentTypeAliases?.TryGetValue(contentTypeKey, out var alias) == true ? alias : string.Empty;

    /// <summary>
    ///  get the key for a given content type alias from the context.
    /// </summary>
    /// <returns>GUID Key value for a content type</returns>
    public Guid GetKeyByAlias(string contentTypeAlias)
        => ContentTypeKeys?.TryGetValue(contentTypeAlias, out var key) == true ? key : Guid.Empty;

    /// <summary>
    ///  return all the content aliases we have loaded in the context.
    /// </summary>
    /// <returns>array of aliases</returns>
    public string[] GetAllAliases()
        => ContentTypeAliases?.Values?.ToArray() ?? Array.Empty<string>();

    /// <summary>
    ///  add content type compositions to the context
    /// </summary>
    public void AddCompositions(string? contentTypeAlias, IEnumerable<string>? compositionAliases)
    {
        _ = string.IsNullOrWhiteSpace(contentTypeAlias) == false &&
            compositionAliases?.Any() == true &&
            ContentTypeCompositions.TryAdd(contentTypeAlias, compositionAliases.ToHashSet());
    }

    /// <summary>
    ///  gets the list of aliases for compositions of a content type
    /// </summary>
    /// <param name="contentTypeAlias">Alias of the content type</param>
    /// <param name="compositionAliases">Enumerable of alises for the compositions</param>
    /// <returns>true if succeeded.</returns>
    public bool TryGetCompositionsByAlias(string? contentTypeAlias, out IEnumerable<string>? compositionAliases)
    {
        compositionAliases = null;

        if (contentTypeAlias != null && ContentTypeCompositions.TryGetValue(contentTypeAlias, out var compositions))
        {
            compositionAliases = compositions?.ToArray();
        }

        return compositionAliases != null;
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
            PropertyTypes.TryAdd($"{contentTypeAlias}_{propertyAlias}",
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
    public EditorAliasInfo? GetEditorAliasByTypeAndProperty(string contentType, string propertyAlias)
    {
        if (PropertyTypes?.TryGetValue($"{contentType}_{propertyAlias}", out var alias) == true)
        {
            return alias;
        }
        else if (TryGetEditorAliasByComposition(contentType, propertyAlias, out var alias1) == true)
        {
            return alias1;
        }

        return null;
    }

    /// <summary>
    ///  returns the editor alias from a property within a composition of a content type.
    /// </summary>
    private bool TryGetEditorAliasByComposition(string compositionKey, string propertyAlias, out EditorAliasInfo? editorAliasInfo)
    {
        if (ContentTypeCompositions?.TryGetValue(compositionKey, out var compositions) == true)
        {
            foreach (var composition in compositions)
            {
                if (PropertyTypes?.TryGetValue($"{composition}_{propertyAlias}", out var alias) == true)
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
    public bool IsElementType(Guid key) => ElementContentTypes.Contains(key);

    /// <summary>
    ///  add an element type to the list of element types.
    /// </summary>
    public void AddElementType(Guid key)
    {
        if (!ElementContentTypes.Contains(key)) ElementContentTypes.Add(key);
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

            var contentTypeAlias = GetAliasByKey(contentTypeKey);

            if (!TryGetCompositionsByAlias(contentTypeAlias, out var compositionAliases))
            {
                continue;
            }

            if (compositionAliases?.Any() != true)
            {
                continue;
            }

            foreach (var compositionContentTypeAlias in compositionAliases)
            {
                var compositionContentTypeKey = GetKeyByAlias(compositionContentTypeAlias);
                AddElementType(compositionContentTypeKey);
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
        => _ = IgnoredProperties.Add($"{contentType}_{alias}");

    /// <summary>
    ///  add a property to ignore for all content types.
    /// </summary>
    public void AddIgnoredProperty(string alias)
    => _ = IgnoredProperties.Add($"{alias}");

    /// <summary>
    ///  returns true if a property is to be ignored 
    /// </summary>
    public bool IsIgnoredProperty(string contentType, string alias)
        => IgnoredProperties.Contains($"{contentType}_{alias}")
        || IgnoredProperties.Contains(alias);

    /// <summary>
    ///  add a new content type - will then be processed as part of the 
    ///  migration process.
    /// </summary>
    public void AddNewContentType(NewContentTypeInfo newDocTypeInfo)
    {
        if (!NewDocTypes.ContainsKey(newDocTypeInfo.Alias))
            NewDocTypes.Add(newDocTypeInfo.Alias, newDocTypeInfo);
    }

    /// <summary>
    ///  list of all the new content types to be created. 
    /// </summary>
    /// <returns></returns>
    public IList<NewContentTypeInfo> GetNewContentTypes()
        => NewDocTypes.Values.ToList();

    /// <summary>
    ///  add a block editor to by name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="alias"></param>
    public void AddBlockEditor(string name, string alias)
        => BlockAliases.TryAdd(name, alias);

    /// <summary>
    ///  get the block editor alias from the name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string GetBlockEditorAliasByName(string name)
        => BlockAliases.TryGetValue(name, out var alias) == true
            ? alias
            : name;

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
        => _ = DataTypeAliases.TryAdd($"{contentTypeAlias}_{propertyAlias}", dataTypeAlias);

    /// <summary>
    ///  get the datatype alias for a property on a content type.
    /// </summary>
    /// <param name="contentTypeAlias"></param>
    /// <param name="propertyAlias"></param>
    /// <returns></returns>
    public string GetDataTypeAlias(string contentTypeAlias, string propertyAlias)
        => DataTypeAliases.TryGetValue($"{contentTypeAlias}_{propertyAlias}", out var alias) == true
            ? alias : string.Empty;

    /// <summary>
    /// Add a replacement alias for a content type alias
    /// </summary>
    public void AddReplacementAlias(string original, string replacement)
        => ReplacementAliases.TryAdd(original, replacement);

    /// <summary>
    ///  get the replacement alias for an property based on the current alias.
    /// </summary>
    /// <param name="alias"></param>
    /// <returns></returns>
    public string GetReplacementAlias(string alias)
        => ReplacementAliases.TryGetValue(alias, out var replacement)
            ? replacement : alias;
}
