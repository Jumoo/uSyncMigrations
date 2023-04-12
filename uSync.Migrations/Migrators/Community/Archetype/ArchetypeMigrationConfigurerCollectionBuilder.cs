using Umbraco.Cms.Core.Composing;

namespace uSync.Migrations.Migrators.Community.Archetype;

public class ArchetypeMigrationConfigurerCollectionBuilder
    : LazyCollectionBuilderBase<ArchetypeMigrationConfigurerCollectionBuilder, ArchetypeMigrationConfigurerCollection, IArchetypeMigrationConfigurer>
{
    protected override ArchetypeMigrationConfigurerCollectionBuilder This => this;
}

public class ArchetypeMigrationConfigurerCollection
    : BuilderCollectionBase<IArchetypeMigrationConfigurer>
{
    public ArchetypeMigrationConfigurerCollection(Func<IEnumerable<IArchetypeMigrationConfigurer>> items) : base(items)
    { }
}