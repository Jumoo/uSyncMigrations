using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;

namespace uSync.Migrations.Migrators.Content;
public class ContentPropertyMigrationCollectionBuilder
    : LazyCollectionBuilderBase<ContentPropertyMigrationCollectionBuilder,
        ContentPropertyMigrationCollection, ISyncContentPropertyMigrator>
{
    protected override ContentPropertyMigrationCollectionBuilder This => this;
}

public class ContentPropertyMigrationCollection :
    BuilderCollectionBase<ISyncContentPropertyMigrator>
{
    public ContentPropertyMigrationCollection(
        Func<IEnumerable<ISyncContentPropertyMigrator>> items) : base(items)
    { }

    public ISyncContentPropertyMigrator? GetMigrator(string editorAlias)
        => this.FirstOrDefault(x => x.Editors.InvariantContains(editorAlias));
}
