﻿using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

using uSync.Migrations.Core.Legacy.Grid;
using uSync.Migrations.Migrators.BlockGrid.Extensions;

namespace uSync.Migrations.Migrators.BlockGrid.BlockMigrators;

/// <summary>
///  base for 'simple' grid things (like text, quote, etc) they all follow a pattern.
/// </summary>
public abstract class GridBlockMigratorSimpleBase
{
    protected readonly IShortStringHelper _shortStringHelper;

    public GridBlockMigratorSimpleBase(IShortStringHelper shortStringHelper)
    {
        _shortStringHelper = shortStringHelper;
    }

    public abstract string GetEditorAlias(ILegacyGridEditorConfig editor);

    /// <summary>
    ///  for the simple migrators we create a new doctype to try and migrate the data into.
    /// </summary>
    /// <param name="editorConfig"></param>
    /// <returns></returns>
    public IEnumerable<NewContentTypeInfo> AdditionalContentTypes(ILegacyGridEditorConfig editor)
    {
        var alias = this.GetContentTypeAlias(editor);

        return new NewContentTypeInfo(
            alias.ToGuid(),
            alias,
            editor.Name ?? editor.Alias!,
            $"{editor.Icon ?? "icon-book"} color-purple",
            "BlockGrid/Elements")
        {
            Description = $"Converted from Grid {editor.Name} element",
            IsElement = true,
            Properties = new List<NewContentTypeProperty>
            {
                new NewContentTypeProperty(
                    alias: editor.Alias!,
                    name: editor.Name ?? editor.Alias!,
                    dataTypeAlias: this.GetEditorAlias(editor))
            }
        }.AsEnumerableOfOne();
    }

    public virtual IEnumerable<string> GetAllowedContentTypes(ILegacyGridEditorConfig config, SyncMigrationContext context)
        => GetContentTypeAlias(config).AsEnumerableOfOne();

    public virtual string GetContentTypeAlias(LegacyGridValue.LegacyGridControl control)
        => control.Editor.Alias.GetBlockElementContentTypeAlias(_shortStringHelper);

    public virtual string GetContentTypeAlias(ILegacyGridEditorConfig editorConfig)
    {
        var alias = string.IsNullOrEmpty(editorConfig.Alias)
            ? Path.GetFileNameWithoutExtension(editorConfig.View) ?? editorConfig.Alias!
            : editorConfig.Alias!;

        return alias.GetBlockElementContentTypeAlias(_shortStringHelper);
    }

    public virtual Dictionary<string, object> GetPropertyValues(LegacyGridValue.LegacyGridControl control, SyncMigrationContext context)
    {
        return new Dictionary<string, object>
        {
            { control.Editor.Alias, control.Value ?? string.Empty }
        };
    }
}


