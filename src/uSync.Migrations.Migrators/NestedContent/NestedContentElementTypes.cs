namespace uSync.Migrations.Migrators.NestedContent;

/// <summary>
///  keys for tracking the doc types that nested content uses, so they
///  can be made element types when they are imported.
/// </summary>
/// <remarks>
///  nested content would let you use any doc type, block list will only
///  use element types. The datatype is imported before the content types,
///  so we record the aliases then, and flip the content type on its way in.
/// </remarks>
internal static class NestedContentElementTypes
{
    private const string _keyPrefix = "NestedContentElement:";

    public static string GetTrackingKey(string contentTypeAlias)
        => $"{_keyPrefix}{contentTypeAlias.ToLowerInvariant()}";
}
