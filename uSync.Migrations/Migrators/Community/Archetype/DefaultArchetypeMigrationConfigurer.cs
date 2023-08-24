using Archetype.Models;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;
using uSync.Migrations.Context;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators.Community.Archetype;

public class DefaultArchetypeMigrationConfigurer : IArchetypeMigrationConfigurer
{
    private readonly IOptions<ArchetypeMigrationOptions> _options;
    private readonly IShortStringHelper _helper;

    public DefaultArchetypeMigrationConfigurer(IOptions<ArchetypeMigrationOptions> options, IShortStringHelper helper)
    {
        
        _options = options;
        _helper = helper;
    }

    public string GetBlockElementAlias(ArchetypeFieldsetModel archetypeAlias,
        SyncMigrationContentProperty dataTypeProperty, SyncMigrationContext context)
    {
        var alias = archetypeAlias.Alias +
                    (!_options.Value.NotMergableDocumentTypes?.Contains(archetypeAlias.Alias) == true
                        ? string.Empty
                        : dataTypeProperty.ContentTypeAlias.ToCleanString(_helper, CleanStringType.UnderscoreAlias));
            
            
        return PrepareAlias(alias);
    }

    public string GetBlockElementAlias(ArchetypePreValueFieldset archetypeAlias, SyncMigrationDataTypeProperty dataTypeProperty,
        SyncMigrationContext context)
    {
        var alias = archetypeAlias.Alias +
                    (!_options.Value.NotMergableDocumentTypes?.Contains(archetypeAlias.Alias) == true
                        ? string.Empty
                        : dataTypeProperty.DataTypeAlias.ToCleanString(_helper, CleanStringType.UnderscoreAlias));
        return PrepareAlias(alias);
    }

    private string PrepareAlias(string alias)
    {
        var renamedDoctypes = _options.Value.RenamedDocumentTypesAliases;
        if (renamedDoctypes!= null && renamedDoctypes.TryGetValue(alias, out var prepareAlias))
        {
            return prepareAlias;
        }

        return alias;
    }
}
