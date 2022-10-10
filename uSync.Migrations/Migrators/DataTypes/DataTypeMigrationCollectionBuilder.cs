using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;

namespace uSync.Migrations.Migrators.DataTypes;
public class DataTypeMigrationCollectionBuilder
    : LazyCollectionBuilderBase<DataTypeMigrationCollectionBuilder, 
        DataTypeMigrationCollection, ISyncDataTypeMigrator>
{
    protected override DataTypeMigrationCollectionBuilder This => this;
}

public class DataTypeMigrationCollection :
    BuilderCollectionBase<ISyncDataTypeMigrator>
{
    public DataTypeMigrationCollection(
        Func<IEnumerable<ISyncDataTypeMigrator>> items) : base(items)
    { }

    public ISyncDataTypeMigrator? GetMigrator(string editorAlias)
        => this.FirstOrDefault(x => x.Editors.InvariantContains(editorAlias));
}
