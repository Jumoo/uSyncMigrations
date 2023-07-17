using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.TinyMce, typeof(RichTextConfiguration), IsDefaultAlias = true)]
[SyncMigrator("Umbraco.TinyMCEv3")]
public class RichTextBoxMigrator : SyncPropertyMigratorBase
{
    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        var richTextValue = string.Empty;

        if (string.IsNullOrWhiteSpace(contentProperty.Value) == false)
            richTextValue = GuidExtensions.LocalLink2Udi(contentProperty.Value);

        return richTextValue;
    }
}