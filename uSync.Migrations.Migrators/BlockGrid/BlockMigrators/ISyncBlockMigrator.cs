﻿using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

using uSync.Migrations.Core.Legacy.Grid;
namespace uSync.Migrations.Migrators.BlockGrid.BlockMigrators;
public interface ISyncBlockMigrator
{
    // the grid aliases this block migrator work for.,
    string[] Aliases { get; }

    IEnumerable<NewContentTypeInfo> AdditionalContentTypes(ILegacyGridEditorConfig editorConfig);
    IEnumerable<string> GetAllowedContentTypes(ILegacyGridEditorConfig config, SyncMigrationContext context);

    string GetContentTypeAlias(LegacyGridValue.GridControl control);
    string GetContentTypeAlias(ILegacyGridEditorConfig editorConfig);
    string GetEditorAlias(ILegacyGridEditorConfig editor);

    Dictionary<string, object> GetPropertyValues(LegacyGridValue.GridControl control, SyncMigrationContext context);
}


public class SyncBlockMigratorCollectionBuilder
    : LazyCollectionBuilderBase<SyncBlockMigratorCollectionBuilder, SyncBlockMigratorCollection, ISyncBlockMigrator>
{
    protected override SyncBlockMigratorCollectionBuilder This => this;
}


public class SyncBlockMigratorCollection
    : BuilderCollectionBase<ISyncBlockMigrator>
{
    public SyncBlockMigratorCollection(Func<IEnumerable<ISyncBlockMigrator>> items) : base(items)
    { }

    public Dictionary<string, ISyncBlockMigrator> GetDefaults()
    {
        var defaults = new Dictionary<string, ISyncBlockMigrator>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in this.Where(x => x.GetType().GetCustomAttribute<SyncDefaultMigratorAttribute>(false) != null))
        {
            foreach (var alias in item.Aliases)
            {
                defaults[alias] = item;
            }
        }

        return defaults;
    }

    public ISyncBlockMigrator? GetMigrator(string? gridAlias)
    {
        if (gridAlias == null) return null;

        var defaults = GetDefaults();
        if (defaults.TryGetValue(gridAlias, out var migrator))
        {
            return migrator;
        }

        return this.FirstOrDefault(x => x.Aliases.InvariantContains(gridAlias));
    }

    public ISyncBlockMigrator? GetDefaultMigrator()
        => GetMigrator("___default___");

    public ISyncBlockMigrator? GetMigrator(GridValue.GridEditor gridEditor)
    {

        ISyncBlockMigrator? migrator;

        var viewName = Path.GetFileNameWithoutExtension(gridEditor?.View ?? "");
        if (!string.IsNullOrWhiteSpace(viewName))
        {
            migrator = GetMigrator(viewName);
            if (migrator != null) return migrator;
        }

        migrator = GetMigrator(gridEditor?.Alias);
        if (migrator != null) return migrator;

        return GetDefaultMigrator();
    }

    public ISyncBlockMigrator? GetMigrator(ILegacyGridEditorConfig editorConfig)
    {
        // we look in a number of places when in a grid editor

        ISyncBlockMigrator? migrator;

        // 1. view
        var viewName = Path.GetFileNameWithoutExtension(editorConfig?.View ?? "");
        if (!string.IsNullOrWhiteSpace(viewName))
        {
            migrator = GetMigrator(viewName);
            if (migrator != null) return migrator;
        }

        // 2. alias
        migrator = GetMigrator(editorConfig?.Alias);
        if (migrator != null) return migrator;

        // if it goes wrong , we return the default, and everything becomes a label.
        return GetDefaultMigrator();
    }
}
