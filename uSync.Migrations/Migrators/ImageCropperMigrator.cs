using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Umbraco.Cms.Core.PropertyEditors;

using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;
public class ImageCropperMigrator : SyncMigratorBase
{
    public override string[] Editors => new[] { "Umbraco.ImageCropper" };

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues)
        => new ImageCropperConfiguration().MapPreValues(preValues);
}
