using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

public class ImageCropperMigrator : SyncPropertyMigratorBase
{
    public override string[] Editors => new[] { UmbConstants.PropertyEditors.Aliases.ImageCropper };

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues, SyncMigrationContext context)
        => new ImageCropperConfiguration().MapPreValues(preValues);
}
