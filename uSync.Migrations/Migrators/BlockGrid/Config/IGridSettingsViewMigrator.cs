using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.BlockGrid.Config
{
    public interface IGridSettingsViewMigrator : IDiscoverable
    {
        string ViewKey { get; }
        string NewDataTypeAlias { get; }
        public object ConvertContentString(string value);
    }

    public class GridSettingsViewMigratorCollectionBuilder
        : LazyCollectionBuilderBase<GridSettingsViewMigratorCollectionBuilder, GridSettingsViewMigratorCollection, IGridSettingsViewMigrator>
    {
        protected override GridSettingsViewMigratorCollectionBuilder This => this;
    }

    public class GridSettingsViewMigratorCollection
        : BuilderCollectionBase<IGridSettingsViewMigrator>
    {
        public GridSettingsViewMigratorCollection(Func<IEnumerable<IGridSettingsViewMigrator>> items) : base(items)
        {}

        public IGridSettingsViewMigrator? GetMigrator(string? viewKey)
        {
            if (viewKey == null) return null;
            return this.FirstOrDefault(x => x.ViewKey.InvariantEquals(viewKey));
        }
    }
}
