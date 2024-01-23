﻿using System.Xml.Linq;

using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

using uSync.Core;
using uSync.Migrations.Core.Context;
using uSync.Migrations.Core.Extensions;
using uSync.Migrations.Core.Handlers.Shared;
using uSync.Migrations.Core.Services;

namespace uSync.Migrations.Core.Handlers.Eight;

public class ContentBaseMigrationHandler<TEntity> : SharedContentBaseHandler<TEntity>
    where TEntity : ContentBase
{
    public ContentBaseMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        IShortStringHelper shortStringHelper,
        ILogger<ContentBaseMigrationHandler<TEntity>> logger)
        : base(eventAggregator, migrationFileService, shortStringHelper, logger)
    { }

    protected override string GetContentType(XElement source, SyncMigrationContext context)
        => context.ContentTypes.GetReplacementAlias(source.Element("Info")?.Element("ContentType").ValueOrDefault(string.Empty) ?? string.Empty);

    protected override int GetId(XElement source) => -1;

    protected override string GetNewContentType(string contentType, XElement source) => contentType;

    protected override Guid GetParent(XElement source)
        => source.Element("Info")?.Element("Parent")?.Attribute("Key").ValueOrDefault(Guid.Empty) ?? Guid.Empty;

    protected override string GetPath(string alias, Guid parent, SyncMigrationContext context)
        => context.Content.GetContentPath(parent) + "/" + alias.ToSafeAlias(_shortStringHelper);

    protected override IEnumerable<XElement>? GetProperties(XElement source)
        => source.Element("Properties")?.Elements() ?? Enumerable.Empty<XElement>();

    protected override string? GetEntityType()
        => null;

    protected override XElement GetBaseXml(XElement source, Guid parent, string contentType, int level, SyncMigrationContext context)
    {
        var target = new XElement(source.Name.LocalName,
                        new XAttribute("Key", source.GetKey()),
                        new XAttribute("Alias", source.GetAlias()),
                        new XAttribute("Level", source.GetLevel()));

        var sourceInfo = source.Element("Info");
        if (sourceInfo != null)
        {
            var targetInfo = sourceInfo.Clone();
            target.Add(targetInfo);
        }

        return target;
    }

}
