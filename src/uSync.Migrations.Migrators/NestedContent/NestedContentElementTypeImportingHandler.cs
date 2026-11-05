using System.Xml.Linq;

using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;

using uSync.BackOffice;
using uSync.Core;
using uSync.Migrations.Core.Tracking;

namespace uSync.Migrations.Migrators.NestedContent;

/// <summary>
///  makes the doc types used by nested content element types as they are imported.
/// </summary>
/// <remarks>
///  Block List only allows element types, nested content allowed any doc type, so
///  without this, the migrated block list would point at doc types it can't use.
///
///  we change the XML before it is imported, so the doc type is saved as an element
///  type, and it stays that way when the same (legacy) file is imported again.
/// </remarks>
internal class NestedContentElementTypeImportingHandler : INotificationAsyncHandler<uSyncImportingItemNotification>
{
    private readonly ISyncMigrationTrackingService _trackingService;
    private readonly IContentTypeService _contentTypeService;
    private readonly ILogger<NestedContentElementTypeImportingHandler> _logger;

    public NestedContentElementTypeImportingHandler(
        ISyncMigrationTrackingService trackingService,
        IContentTypeService contentTypeService,
        ILogger<NestedContentElementTypeImportingHandler> logger)
    {
        _trackingService = trackingService;
        _contentTypeService = contentTypeService;
        _logger = logger;
    }

    public async Task HandleAsync(uSyncImportingItemNotification notification, CancellationToken cancellationToken)
    {
        var node = notification.Item;
        if (node is null || node.Name.LocalName != "ContentType") return;

        var info = node.Element("Info");
        if (info is null) return;

        if (info.Element("IsElement").ValueOrDefault(false)) return;

        var alias = node.GetAlias();
        if (string.IsNullOrWhiteSpace(alias)) return;

        if (await _trackingService.ExistsAsync(NestedContentElementTypes.GetTrackingKey(alias)) is false)
            return;

        // a doc type that can be created at the root is being used as a page,
        // making it an element would stop that working.
        if (info.Element("AllowAtRoot").ValueOrDefault(false))
        {
            _logger.LogWarning("MIGRATION WARNING: Content type {alias} is used in Nested Content but is allowed at the root, so it has not been made an element type. Block List will not be able to use it until it is.", alias);
            return;
        }

        // likewise if there is already content of this type in the site.
        var existing = _contentTypeService.Get(node.GetKey()) ?? _contentTypeService.Get(alias);
        if (existing is not null && existing.IsElement is false && _contentTypeService.HasContentNodes(existing.Id))
        {
            _logger.LogWarning("MIGRATION WARNING: Content type {alias} is used in Nested Content but has content nodes, so it has not been made an element type. Block List will not be able to use it until it is.", alias);
            return;
        }

        var isElement = info.Element("IsElement");
        if (isElement is null)
            info.Add(new XElement("IsElement", true));
        else
            isElement.Value = "true";

        _logger.LogInformation("Migrating: Content type {alias} is used in Nested Content, making it an element type for Block List", alias);
    }
}
