using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Models;

namespace MyMigrations.Mergers;
internal class SimpleMergePlan : ISyncMigrationPlan
{
    public int Order => 20;
    public string Name => "Simple merger plan";
    public string Icon => "icon-merge";
    public string Description => "Will merge string values";

    public MigrationOptions Options => new MigrationOptions
    {
        SourceVersion = 8, // only run on v8 to v10/11 migrations

        MergingProperties = new()
        {
            {
                // merge the footer on the home content type
                "home", new MergingPropertiesConfig("mergedFooterProperties",
                                nameof(SimpleStringMerger),
                                new List<string>
                                {
                                    "footerDescription", "footerAddress", "footerHeader", "FooterCtalink", "footerCTACaption"
                                })

            }
        }
    };

    // setting the values in the constructor might be simpler to read. 

    //public SimpleMergePlan()
    //{
    //    var propertiesToMerge = new[] { "footerDescription", "footerAddress", "footerHeader", "FooterCtalink", "footerCTACaption" };
    //    Options.MergingProperties.Add("Home", new MergingPropertiesConfig("mergedFooterProperties", nameof(SimpleStringMerger), propertiesToMerge));
    //    Options.MergingProperties.Add("Person", new MergingPropertiesConfig("mergedPersonProperties", nameof(SimpleStringMerger), propertiesToMerge));
    //}

}
