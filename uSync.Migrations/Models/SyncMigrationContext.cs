namespace uSync.Migrations.Models;

/// <summary>
///  A uSync migration context, lets us keep a whole list of things in memory while we do the migration.
/// </summary>
public class SyncMigrationContext
{
    private HashSet<string> _blockedTypes = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<Guid, string> _contentKeys { get; set; } = new();
    private Dictionary<Guid, string> _contentPaths { get; set; } = new();
    private Dictionary<string, Guid> _contentTypeKeys { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, HashSet<string>> _contentTypeCompositions { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, string> _propertyTypes { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, Guid> _templateKeys { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public SyncMigrationContext(Guid migrationId)
    {
        MigrationId = migrationId;
    }

    public Guid MigrationId { get; }

    public void AddTemplateKey(string templateAlias, Guid templateKey)
         => _ = _templateKeys.TryAdd(templateAlias, templateKey);

    public Guid GetTemplateKey(string templateAlias)
         => _templateKeys?.TryGetValue(templateAlias, out var key) == true ? key : Guid.Empty;

    public void AddContentTypeKey(string? contentTypeAlias, Guid? contentTypeKey)
    {
        _ = string.IsNullOrWhiteSpace(contentTypeAlias) == false &&
            contentTypeKey.HasValue == true &&
            _contentTypeKeys.TryAdd(contentTypeAlias, contentTypeKey.Value);
    }

    public void AddContentTypeCompositions(string? contentTypeAlias, IEnumerable<string>? compositionAliases)
    {
        _ = string.IsNullOrWhiteSpace(contentTypeAlias) == false &&
            compositionAliases?.Any() == true &&
            _contentTypeCompositions.TryAdd(contentTypeAlias, compositionAliases.ToHashSet());
    }

    public Guid GetContentTypeKey(string contentTypeAlias)
        => _contentTypeKeys?.TryGetValue(contentTypeAlias, out var key) == true ? key : Guid.Empty;

    public void AddContentPath(Guid key, string path)
         => _ = _contentPaths.TryAdd(key, path);

    public string GetContentPath(Guid parentKey)
        => _contentPaths?.TryGetValue(parentKey, out var path) == true ? path : string.Empty;

    public void AddContentProperty(string? contentTypeAlias, string? propertyAlias, string? editorAlias)
    {
        _ = string.IsNullOrWhiteSpace(contentTypeAlias) == false &&
            string.IsNullOrWhiteSpace(propertyAlias) == false &&
            string.IsNullOrWhiteSpace(editorAlias) == false &&
            _propertyTypes.TryAdd($"{contentTypeAlias}_{propertyAlias}", editorAlias);
    }

    public string GetEditorAlias(string contentType, string propertyAlias)
    {
        if (_propertyTypes?.TryGetValue($"{contentType}_{propertyAlias}", out var alias) == true)
        {
            return alias;
        }
        else if (_contentTypeCompositions?.TryGetValue(contentType, out var compositions) == true)
        {
            foreach (var composition in compositions)
            {
                if (_propertyTypes?.TryGetValue($"{composition}_{propertyAlias}", out var alias1) == true)
                {
                    return alias1;
                }
            }
        }

        return string.Empty;
    }

    public void AddContentKey(Guid key, string alias)
        => _ = _contentKeys.TryAdd(key, alias);

    public string GetContentAlias(Guid key)
        => _contentKeys?.TryGetValue(key, out var alias) == true ? alias : string.Empty;

    public bool IsBlocked(string itemType, string alias)
        => _blockedTypes.Contains($"{itemType}_{alias}") == true;

    public void AddBlocked(string itemType, string alias)
        => _ = _blockedTypes.Add($"{itemType}_{alias}");
}
