using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using uSync.Migrations.Context;
using uSync.Migrations.Migrators;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Models;

namespace MyMigrations.Mergers;

/// <summary>
///  example merging migrator. 
/// </summary>
/// <remarks>
///  merging migrators will likey be very project specific, 
///  if you want to merge to properties into one, you 
///  need to configure that in a plan, and then 
///  this migrator would get a list of properties, and 
///  it can work out how to merge them into on single propery.
/// </remarks>
internal class SimpleStringMerger : ISyncPropertyMergingMigrator
{
    public string[] ContentTypes => throw new NotImplementedException();

    /// <summary>
    ///  Get the property info for the thing we are going to merge.
    /// </summary>
    /// <remarks>
    ///  you don't have to implement this, if you already have the 
    ///  property to want to merge into setup - but if for example
    ///  you wanted to create a whole new property (or datatype or anything)
    ///  as part of the migration you could do that here, and it will 
    ///  become part of the migration.
    /// </remarks>
    public SplitPropertyInfo GetMergedProperty(string contentTypeAlias, string propertyAlias, string propertyName, SyncMigrationContext context)
    {
        // a simplistic example, we will create a new property on 
        // the content type, called "Merged strings"
        return new SplitPropertyInfo("Merged Strings",
            "mergedStrings",
            "Umbraco.TextArea",
            context.DataTypes.GetFirstDefinition("Umbraco.TextArea") ?? Guid.Parse("0cc0eba1-9960-42c9-bf9b-60e150b429ae"));
    }

    public string GetMergedContentValues(IEnumerable<MergingPropertyValue> mergingPropertyValues, SyncMigrationContext context)
    {
        // this merger just merges everything into one single text field.
        return string.Join("\r\n", mergingPropertyValues.Select(x => x.Value));
    }

}
