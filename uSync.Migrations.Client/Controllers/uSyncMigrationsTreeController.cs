﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.ModelBinders;

using uSync.Migrations.Core;

namespace uSync.Migrations.Client.Controllers;

[Tree(UmbConstants.Applications.Settings,
    uSyncMigrationsClient.TreeName,
    TreeGroup = "sync",
    TreeTitle = "uSync Migrations",
    SortOrder = 99)]
[PluginController(uSyncMigrationsClient.TreeName)]
public class uSyncMigrationsTreeController : TreeController
{
    private readonly IMenuItemCollectionFactory _menuItemCollectionFactory;

    public uSyncMigrationsTreeController(
        ILocalizedTextService localizedTextService,
        UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
        IEventAggregator eventAggregator,
        IMenuItemCollectionFactory menuItemCollectionFactory)
        : base(localizedTextService, umbracoApiControllerTypeCollection, eventAggregator)
    {
        _menuItemCollectionFactory = menuItemCollectionFactory;
    }

    protected override ActionResult<TreeNode?> CreateRootNode(FormCollection queryStrings)
    {
        var root = base.CreateRootNode(queryStrings);

        if (root.Value != null)
        {
            root.Value.RoutePath = $"{SectionAlias}/{uSyncMigrationsClient.TreeName}/dashboard";
            root.Value.Icon = uSyncMigrationsClient.Icon;
            root.Value.HasChildren = false;
            root.Value.MenuUrl = null;
        }

        return root.Value;
    }


    protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormCollection queryStrings)
    {
        return _menuItemCollectionFactory.Create();
    }

    protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))] FormCollection queryStrings)
    {
        return new TreeNodeCollection();
    }
}
