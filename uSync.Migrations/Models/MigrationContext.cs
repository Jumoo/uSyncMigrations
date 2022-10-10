namespace uSync.Migrations.Models;
public class MigrationContext
{
    private List<string> _blockedTypes = new List<string>();

    private Dictionary<string, Guid> _templateKeys { get; set; }
        = new Dictionary<string, Guid>();
    private Dictionary<string, Guid> _contentTypeKeys { get; set; }
        = new Dictionary<string, Guid>();
    private Dictionary<Guid, string> _contentPaths { get; set; }
        = new Dictionary<Guid, string>();
    private Dictionary<string, string> _propertyTypes { get; set; }
        = new Dictionary<string, string>();
    private Dictionary<Guid, string> _contentKeys { get; set; } 
        = new Dictionary<Guid, string>();

    public Guid GetTemplateKey(string templateAlias)
    {
        if (_templateKeys != null && _templateKeys.ContainsKey(templateAlias))
            return _templateKeys[templateAlias];

        return Guid.Empty;
    }

    public void AddTemplateKey(string templateAlias, Guid templateKey)
    {
        _templateKeys[templateAlias] = templateKey;
    }

    public Guid GetContentTypeKey(string contentTypeAlias)
    {
        if (_contentTypeKeys != null && _contentTypeKeys.ContainsKey(contentTypeAlias))
            return _contentTypeKeys[contentTypeAlias];

        return Guid.Empty;
    }

    public void AddContentTypeKey(string contentTypeAlias, Guid contentTypeKey)
    {
        _contentTypeKeys[contentTypeAlias] = contentTypeKey;
    }

    public string GetContentPath(Guid parentKey)
    {
        if (_contentPaths != null && _contentPaths.ContainsKey(parentKey))
            return _contentPaths[parentKey];

        return string.Empty;
    }

    public void AddContentPath(Guid key, string path)
    {
        _contentPaths[key] = path;
    }

    public void AddContentProperty(string contentType, string propertyAlias, string editorAlias)
        => _propertyTypes[$"{contentType}_{propertyAlias}"] = editorAlias;

    public string GetEditorAlias(string contentType, string propertyAlias)
    {
        var key = $"{contentType}_{propertyAlias}";
        if (_propertyTypes != null && _propertyTypes.ContainsKey(key))
            return _propertyTypes[key];

        return string.Empty;
    }


    public void AddContentKey(Guid key, string alias)
        => _contentKeys[key] = alias;

    public string GetContentAlias(Guid key)
    {
        if (_contentKeys != null && _contentKeys.ContainsKey(key))
            return _contentKeys[key];

        return String.Empty;
    }


    public bool IsBlocked(string itemType, string alias)
        => _blockedTypes.Contains($"{itemType.ToLower()}_{alias}");

    public void AddBlocked(string itemType, string alias)
        => _blockedTypes.Add($"{itemType.ToLower()}_{alias}");
}
