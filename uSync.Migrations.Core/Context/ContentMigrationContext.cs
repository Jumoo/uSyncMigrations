using System.Diagnostics.CodeAnalysis;

using uSync.Migrations.Core.Models;

namespace uSync.Migrations.Core.Context;
public class ContentMigrationContext
{
    private Dictionary<Guid, string> _contentKeys { get; set; } = new();
    private Dictionary<Guid, string> _contentPaths { get; set; } = new();

    private Dictionary<string, MergingPropertiesConfig> _mergedProperties { get; set; } = new(StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    ///  add the path for a content item to context. 
    /// </summary>
    public void AddContentPath(Guid key, string path)
         => _ = _contentPaths.TryAdd(key, path);

    /// <summary>
    ///  get the content path for a parent item from the context.
    /// </summary>
    public string GetContentPathOrDefault(Guid parentKey, string defaultPath)
        => _contentPaths?.TryGetValue(parentKey, out var path) == true ? path : defaultPath;
  
    /// <summary>
    ///  add a content key to the context.
    /// </summary>
    public void AddKey(Guid key, string alias)
        => _ = _contentKeys.TryAdd(key, alias);

    /// <summary>
    ///  get a context alias from the context
    /// </summary>
    public bool TryGetAliasByKey(Guid key, [MaybeNullWhen(false)] out string alias)
        => _contentKeys.TryGetValue(key, out alias);

    /// <summary>
    ///  a information for when two (or more) properties are being merged into one.
    /// </summary>
    public void AddMergedProperty(string contentType, MergingPropertiesConfig config)
    {
        _ = _mergedProperties.TryAdd(contentType, config);
    }

    /// <summary>
    ///  get details of a merged set of properties. 
    /// </summary>
    public bool TryGetMergedProperties(string contentType, [MaybeNullWhen(false)] out MergingPropertiesConfig properties)
        => _mergedProperties.TryGetValue(contentType, out properties);
}
