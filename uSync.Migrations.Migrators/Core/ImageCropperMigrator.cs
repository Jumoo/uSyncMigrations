using Umbraco.Cms.Core.PropertyEditors;

namespace uSync.Migrations.Migrators.Core;

[SyncMigrator(UmbEditors.Aliases.ImageCropper, typeof(ImageCropperConfiguration))]
public class ImageCropperMigrator : SyncPropertyMigratorBase
{ }