using System.Net;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Extensions;

namespace uSync.Migrations.Controllers;

[Tree(UmbConstants.Applications.Settings, 
    "uSyncFiles",
    TreeTitle = "uSync Files",
    TreeUse = TreeUse.Dialog)]
public class uSyncFilesTreeController : TreeController
{
    private readonly IFileSystem _fileSystem;  
    private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;

    public uSyncFilesTreeController(
        ILocalizedTextService localizedTextService,
        Umbraco.Cms.Core.UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
        IEventAggregator eventAggregator,
        IPhysicalFileSystem fileSystem,
        IMenuItemCollectionFactory menuItemCollectionFactory)
        : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
    {
        _fileSystem = fileSystem;
        _menuItemCollectionFactory = menuItemCollectionFactory;
    }

    private const string uSyncFolder = "uSync";

    protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
    {
        var path = string.IsNullOrEmpty(id) == false && id != UmbConstants.System.RootString
            ? WebUtility.UrlDecode(id).TrimStart("/")
            : "";

        var nodes = new TreeNodeCollection();
        var directories = _fileSystem.GetDirectories(path);

        foreach (var directory in directories)
        {
            // We don't want any other directories under the root node other than the uSync one
            if (id == UmbConstants.System.RootString && directory != uSyncFolder)
            {
                continue;
            }

            var hasChildren = _fileSystem.GetFiles(directory).Any() || _fileSystem.GetDirectories(directory).Any();

            var name = Path.GetFileName(directory);
            var node = CreateTreeNode(WebUtility.UrlEncode(directory), path, queryStrings, name, UmbConstants.Icons.Folder, hasChildren);

            if (node != null)
            {
                nodes.Add(node);
            }
        }

        // Only get the files inside App_Plugins and wwwroot
        var files = _fileSystem.GetFiles(path).Where(x => x.StartsWith(uSyncFolder));

        foreach (var file in files)
        {
            var name = Path.GetFileName(file);
            var node = CreateTreeNode(WebUtility.UrlEncode(file), path, queryStrings, name, UmbConstants.Icons.DefaultIcon, false);

            if (node != null)
            {
                nodes.Add(node);
            }
        }

        return nodes;
    }

    protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings) => _menuItemCollectionFactory.Create();

}
