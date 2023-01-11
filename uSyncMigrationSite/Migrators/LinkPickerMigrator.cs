using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Migrators;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Models;

namespace uSyncMigrationSite;

[SyncMigrator("Novicell.LinkPicker")]
public class LinkPickerMigrator : SyncPropertyMigratorBase
{
    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => Constants.PropertyEditors.Aliases.MultiUrlPicker;

    public override object GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        return new MultiUrlPickerConfiguration
        {
            MinNumber = 0,
            MaxNumber = 1,
            IgnoreUserStartNodes = true,
        };
    }

    public override string GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (string.IsNullOrWhiteSpace(contentProperty.Value))
        {
            return string.Empty;
        }

        var source = JObject.Parse(contentProperty.Value);

        // map source to new value
        var value = new MultiUrlPickerValueEditor.LinkDto
        {
            Name = source?.Value<string>("name"),
            Target = source?.Value<string>("target"),
            Url = source?.Value<string>("url"),
            QueryString = source?.Value<string>("hashtarget"),
        };

        var id = source?.Value<int>("id");

        if (id != null)
        {
            /*
             * An ID is a record identifier from the database. This means that an ID only exists
             * in the original database. A new ID will be generated when content is transfered
             * (via Umbraco Deploy from Umbraco Cloud).
             * To fix this, content must be exported from the actual database, that the content
             * is stored in. This is the same for all properties that use the ID and not
             * KEY of the content/media being referenced.
             */
            
            var key = context.GetKey(id.Value);

            if (key != Guid.Empty)
            {
                value.Udi = new GuidUdi(Constants.UdiEntityType.Document, key);
            }
        }

        if (string.IsNullOrWhiteSpace(value.Url))
        {
            return string.Empty;
        }
        
        return JsonConvert.SerializeObject(new[] { value });
    }
}