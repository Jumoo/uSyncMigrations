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

    private string getMigratedAlias(string archetypeAlias, string typeAlias)
        => archetypeAlias +
                    (!_options.Value.NotMergableDocumentTypes?.Contains(archetypeAlias) == true
                        ? string.Empty
                        : typeAlias.ToCleanString(_helper, CleanStringType.UnderscoreAlias));

    public string GetBlockElementAlias(ArchetypeFieldsetModel archetypeAlias,
        SyncMigrationContentProperty dataTypeProperty, SyncMigrationContext context)
    {
        var alias = getMigratedAlias(archetypeAlias.Alias!, dataTypeProperty.ContentTypeAlias);
        return PrepareAlias(alias);
    }

    public string GetBlockElementAlias(ArchetypePreValueFieldSet archetypeAlias, SyncMigrationDataTypeProperty dataTypeProperty,
        SyncMigrationContext context)
    {
        var alias = getMigratedAlias(archetypeAlias.Alias!, dataTypeProperty.DataTypeAlias);
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
