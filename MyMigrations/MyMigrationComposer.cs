using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

using uSync.Migrations;
using uSync.Migrations.Migrators;

namespace MyMigrations;

/// <summary>
///  this is an example of how you might extend, and replace
///  core migrations for certain types (e.g the grid)
/// </summary>
/// <remarks>
///  by default the core will convert what it can, but if you
///  want to do something special, you can implement your 
///  own ISyncItemMigrations and remove the core ones .
/// </remarks>

[ComposeAfter(typeof(SyncMigrationComposer))]
public class MyMigrationComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // if we have a custom migration for the grid, we should 
        // remove the existing one. 
        builder.Migrators().Remove<GridMigrator>();

        // our migration will be discovered, if it implements ISyncItemMigrator
    }
}
