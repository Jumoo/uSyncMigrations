using System.Diagnostics.CodeAnalysis;

using Org.BouncyCastle.Bcpg.OpenPgp;

namespace uSync.Migrations.Core.Context;
public class TemplateMigratorContext
{
    private Dictionary<string, Guid> _templateKeys { get; set; } = new(StringComparer.OrdinalIgnoreCase);


    /// <summary>
    ///  Add a template key to the context.
    /// </summary>
    public void AddAliasKeyLookup(string templateAlias, Guid templateKey)
         => _ = _templateKeys.TryAdd(templateAlias, templateKey);

    /// <summary>
    ///  get a template key (Guid) from the context 
    /// </summary>   
    public bool TryGetKeyByAlias(string templateAlias, [MaybeNullWhen(false)] out Guid key)
        => _templateKeys.TryGetValue(templateAlias, out key);
}
