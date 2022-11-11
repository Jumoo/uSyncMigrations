using Umbraco.Cms.Core.PropertyEditors;

namespace uSync.Migrations.Migrators;

[SyncMigrator(UmbConstants.PropertyEditors.Aliases.ImageCropper, typeof(ImageCropperConfiguration))]
public class ImageCropperMigrator : SyncPropertyMigratorBase
{ }