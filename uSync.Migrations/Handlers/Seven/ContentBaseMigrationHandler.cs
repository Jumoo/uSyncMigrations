using System.Xml.Linq;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

using uSync.Core;
using uSync.Migrations.Configuration;
using uSync.Migrations.Context;
using uSync.Migrations.Handlers.Shared;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Seven;

internal abstract class ContentBaseMigrationHandler<TEntity> : SharedContentBaseHandler<TEntity>
    where TEntity : ContentBase
{
    public ContentBaseMigrationHandler(
        IOptions<uSyncMigrationOptions> options,
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        IShortStringHelper shortStringHelper,
        ILogger<ContentBaseMigrationHandler<TEntity>> logger)
        : base(options,eventAggregator, migrationFileService, shortStringHelper, logger)
    { }

    protected override int GetId(XElement source)
        => source.Attribute("id").ValueOrDefault(0);

    protected override (string alias, Guid key) GetAliasAndKey(XElement source)
       => (
            alias: source.Attribute("nodeName").ValueOrDefault(string.Empty),
            key: source.Attribute("guid").ValueOrDefault(Guid.Empty)
        );

    protected override Guid GetParent(XElement source)
        => source.Attribute("parentGUID").ValueOrDefault(Guid.Empty);

    protected override string GetContentType(XElement source)
        => source.Attribute("nodeTypeAlias").ValueOrDefault(string.Empty);

    protected override string GetPath(string alias, Guid parent, SyncMigrationContext context)
        => context.Content.GetContentPath(parent) + "/" + alias.ToSafeAlias(_shortStringHelper);

    protected override IEnumerable<XElement>? GetProperties(XElement source)
        => source.Elements();

    protected override string GetNewContentType(string contentType, XElement source)
    {
        if (ItemType == nameof(Media) && _mediaTypeAliasForFileExtension.Count > 0)
        {
            var fileExtension = source.Element(UmbConstants.Conventions.Media.Extension)?.ValueOrDefault(string.Empty) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fileExtension) == false && _mediaTypeAliasForFileExtension.TryGetValue(fileExtension, out var newMediaTypeAlias) == true)
            {
                return newMediaTypeAlias;
            }
        }

        return contentType;
    }
   

    protected override XElement GetBaseXml(XElement source, Guid parent, string contentType, int level, SyncMigrationContext context)
    {
        var (alias, key) = GetAliasAndKey(source);

        var template = source.Attribute("templateAlias").ValueOrDefault(string.Empty);
        var published = source.Attribute("published").ValueOrDefault(false);
        var createdDate = source.Attribute("updated").ValueOrDefault(DateTime.Now);
        var sortOrder = source.Attribute("sortOrder").ValueOrDefault(0);

        var path = GetPath(alias, parent, context);

        var target = new XElement(ItemType,
                        new XAttribute("Key", key),
                        new XAttribute("Alias", alias),
                        new XAttribute("Level", level),
                        new XElement("Info",
                            new XElement("Parent", new XAttribute("Key", parent), context.Content.GetAliasByKey(parent)),
                            new XElement("Path", path),
                            new XElement("Trashed", false),
                            new XElement("ContentType", contentType),
                            new XElement("CreateDate", createdDate.ToString("s")),
                            new XElement("NodeName", new XAttribute("Default", alias)),
                            new XElement("SortOrder", sortOrder)
                        )
                    );

        if (ItemType == nameof(Content))
        {
            var info = target.Element("Info");
            if (info != null)
            {
                info.Add(new XElement("Published", new XAttribute("Default", published)));
                info.Add(new XElement("Schedule"));

                if (string.IsNullOrWhiteSpace(template) == false)
                {
                    info.Add(new XElement("Template", new XAttribute("Key", context.Templates.GetKeyByAlias(template)), template));
                }
                else
                {
                    info.Add(new XElement("Template"));
                }
            }
        }
        return target;
    }
}
