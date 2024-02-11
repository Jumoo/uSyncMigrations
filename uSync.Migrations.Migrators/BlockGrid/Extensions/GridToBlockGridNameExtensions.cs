﻿using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace uSync.Migrations.Migrators.BlockGrid.Extensions;

internal static class GridToBlockGridNameExtensions
{
    public static string GetBlockElementContentTypeAlias(this string name, IShortStringHelper shortStringHelper)
        => name.GetContentTypeAlias("BlockElement_", shortStringHelper);

    public static string GetBlockSettingsContentTypeAlias(this string name, IShortStringHelper shortStringHelper)
        => name.GetContentTypeAlias("BlockSettings_", shortStringHelper);

    public static string GetBlockGridLayoutContentTypeAlias(this string name, IShortStringHelper shortStringHelper)
        => name.GetContentTypeAlias("BlockGridLayout_", shortStringHelper);

    public static string GetBlockGridAreaConfigurationAlias(this string name, IShortStringHelper shortStringHelper)
        => name.GetContentTypeAlias("BlockGridArea_", shortStringHelper);

    private static string GetContentTypeAlias(this string name, string prefix, IShortStringHelper shortStringHelper)
    {
        return $"{prefix}{name}".ToSafeAlias(shortStringHelper);
    }

    public static Guid GetContentTypeKeyOrDefault(this SyncMigrationContext context, string alias, Guid defaultKey)
    {
        if (context.ContentTypes.TryGetKeyByAlias(alias, out var key) is false)
            return defaultKey; 

        context.Content.AddKey(key, alias);
        return key;
    }
    public static string GetBlockGridLayoutSettingsContentTypeAlias(this string name, IShortStringHelper shortStringHelper)
        => name.GetContentTypeAlias("BlockGridLayoutSettings_", shortStringHelper);

}
