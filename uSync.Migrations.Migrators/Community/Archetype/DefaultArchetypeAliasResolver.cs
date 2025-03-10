using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace uSync.Migrations.Migrators.Community.Archetype;

public class DefaultArchetypeAliasResolver : IArchetypeAliasResolver
{
    private readonly ArchetypeMigrationOptions _options;
    private readonly IShortStringHelper _shortStringHelper;

    public DefaultArchetypeAliasResolver(
        IOptions<ArchetypeMigrationOptions> options,
        IShortStringHelper shortStringHelper)
    {
        _options = options.Value;
        _shortStringHelper = shortStringHelper;
    }

    /// <summary>
    /// Gets the unique block element alias for the provided <paramref name="fieldSetAlias"/> and <paramref name="dataTypeAlias"/>
    /// </summary>
    /// <param name="fieldSetAlias">the archetype fieldset alias</param>
    /// <param name="dataTypeAlias">the data type alias</param>
    /// <returns></returns>
    public string GetBlockElementAlias(string fieldSetAlias, string dataTypeAlias) =>
        GetRenamedAlias(GetUniqueAlias(fieldSetAlias, dataTypeAlias));

    /// <summary>
    /// Locates the alias for the data type
    /// </summary>
    /// <param name="contentProperty"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public string GetDataTypeAlias(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        // Locate based on the content type
        var contentTypeDataTypeAlias = context.ContentTypes
            .GetDataTypeAlias(contentProperty.ContentTypeAlias, contentProperty.PropertyAlias);

        if (!string.IsNullOrEmpty(contentTypeDataTypeAlias))
            return contentTypeDataTypeAlias;

        // Locate based on the composition content type 
        if (context.ContentTypes.TryGetCompositionsByAlias(contentProperty.ContentTypeAlias, out var compositions))
        {
            foreach (var compositionAlias in compositions.EmptyNull())
            {
                var compositionDataTypeAlias = context.ContentTypes
                    .GetDataTypeAlias(compositionAlias, contentProperty.PropertyAlias);

                if (!string.IsNullOrEmpty(compositionDataTypeAlias))
                    return compositionDataTypeAlias;
            }
        }

        // Locate based on the new content types
        var newContentType = context.ContentTypes.GetNewContentTypes()
            .FirstOrDefault(ct => ct.Alias == contentProperty.ContentTypeAlias);

        if (newContentType is null)
            return string.Empty;

        var newPropertyType = newContentType.Properties
            .FirstOrDefault(pt => pt.Alias == contentProperty.PropertyAlias);

        if (newPropertyType is not null)
            return newPropertyType.DataTypeAlias;

        return string.Empty;
    }

    /// <summary>
    /// Filters out document types marked as not mergable and generates a unique alias
    /// </summary>
    /// <param name="fieldSetAlias"></param>
    /// <param name="dataTypeAlias"></param>
    /// <returns></returns>
    private string GetUniqueAlias(string fieldSetAlias, string dataTypeAlias)
    {
        string suffix = _options.NotMergableDocumentTypes.EmptyNull().Contains(fieldSetAlias)
            ? dataTypeAlias.ToCleanString(_shortStringHelper, CleanStringType.Alias).ToFirstUpper()
            : string.Empty;

        return fieldSetAlias.ToFirstLower() + suffix;
    }

    /// <summary>
    /// Switches for the renamed document type given the provided <paramref name="alias"/> 
    /// </summary>
    /// <param name="alias"></param>
    /// <returns></returns>
    private string GetRenamedAlias(string alias)
    {
        if (_options.RenamedDocumentTypesAliases is not null &&
            _options.RenamedDocumentTypesAliases.TryGetValue(alias, out string? renamedAlias))
            return renamedAlias;

        return alias;
    }
}