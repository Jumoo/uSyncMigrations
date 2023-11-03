using uSync.Migrations.Core.Migrators.Models;

namespace uSync.Migrations.Core.Extensions;
internal static class SyncMigrationContentPropertyExtensions
{
    /// <summary>
    ///  logging aid, gives us the details we can log.
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    public static string GetDetailString(this SyncMigrationContentProperty property)
        => string.Format("[{0}-{1} {2}]",
            property?.ContentTypeAlias ?? "(Blank)",
            property?.PropertyAlias ?? "(Blank)",
            property?.EditorAlias ?? "(Blank)");

}
