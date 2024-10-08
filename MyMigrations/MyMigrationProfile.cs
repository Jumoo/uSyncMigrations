using Umbraco.Cms.Core.Models;

using uSync.Migrations.Core;
using uSync.Migrations.Core.Composing;
using uSync.Migrations.Core.Configuration.Models;
using uSync.Migrations.Core.Extensions;
using uSync.Migrations.Migrators.Optional;

namespace MyMigrations;

/***** this code is not hooked up by default, it never runs *****/

/// <summary>
///  A Custom migration profile, to do things in special ways.
/// </summary>

public class MyMigrationProfile : ISyncMigrationPlan
{
    private readonly SyncMigrationHandlerCollection _migrationHandlers;

    public MyMigrationProfile(SyncMigrationHandlerCollection migrationHandlers)
    {
        _migrationHandlers = migrationHandlers;
    }

    public string Name => "My Migration Profile";

    public string Icon => "icon-cloud color-blue";

    public string Description => "My Custom migration with changes";

    public int Order => 10;

    public MigrationOptions Options => new()
    {
        // write out to the same folder each time.
        Target = $"{uSyncMigrations.MigrationFolder}/My-Custom-Migration",

        // load all the handlers just enable the content ones.
        Handlers = _migrationHandlers
                        .Handlers
                        // .Select(x => x.ToHandlerOption(x.Group == uSync.BackOffice.uSyncConstants.Groups.Content))
                        .Select(x => x.ToHandlerOption(true))
                        .ToList(),

        // for this migrator we want to use our special grid migrator.
        PreferredMigrators = new Dictionary<string, string>()
        {
            // { Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.Grid, nameof(GridToBlockListMigrator) }
            { Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.NestedContent, nameof(NestedToBlockListMigrator) }
        },

        // eveything beneath is optional... 

        //PropertyMigrators = new Dictionary<string, string>()
        //{
        //    // use the NestedToBlockListMigrator For myProperty in the 'MyContentType' contentType
        //    { "myContentType_myProperty", nameof(NestedToBlockListMigrator) }, 

        //    // Convert all properties called myGridProperty to blocklist 
        //    { "myGridProperty", nameof(GridToBlockListMigrator) }
        //},


        // add a list of things we don't want to import 
        BlockedItems = new Dictionary<string, List<string>>
        {
            { nameof(DataType),
                // Specify DataTypes here, not property editors
                new List<string> {
                    "Custom Legacy Type", "My BoxGrid Things"
                }
            }
        },

        // add a list of properties we are ignoring on all content
        IgnoredProperties = new List<string>
        {
            "SeoMetaDescription", "SeoToastPopup", "Keywords"
        },

        // add things we only want to ignore on certain types

        IgnoredPropertiesByContentType = new Dictionary<string, List<string>>
        {
            { "HomePage", new List<string>
                {
                    "SiteName", "GoogleAnalyticsCode"
                }
            }
        },

        // change the tabs around a bit if needed/
        ChangeTabs = new List<TabOptions>
        {
            {
                //Rename the Meta Data tab to SEO with the alias of seo
                new TabOptions (
                    originalName: "Meta Data",
                    newName: "SEO",
                    alias: "seo")
            },
            {
                //Move the contents of the tab Carousel into the Content tab.  If content doesn't exist it will
                //be created with the alias "Content"
                new TabOptions(
                    originalName: "Carousel",
                    newName: "Content",
                    alias: string.Empty)
            },
            {
                //No new name or alias means delete the tab, so delete the "Cookie Law" tab in this example
                new TabOptions(
                    originalName: "Cookie Law",
                    newName: string.Empty,
                    alias:string.Empty)
            }
        },
    };
}
