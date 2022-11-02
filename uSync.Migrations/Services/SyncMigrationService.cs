using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;
using uSync.BackOffice.Configuration;
using uSync.Migrations.Composing;
using uSync.Migrations.Handlers;
using uSync.Migrations.Models;

namespace uSync.Migrations.Services;

public class SyncMigrationService
{
    private readonly SyncMigrationFileService _migrationFileService;
    private readonly SyncMigrationHandlerCollection _migrationHandlers;
    private readonly uSyncConfigService _usyncConfig;

    public SyncMigrationService(
        SyncMigrationFileService migrationFileService,
        SyncMigrationHandlerCollection migrationHandlers,
        uSyncConfigService usyncConfig)
    {
        _migrationFileService = migrationFileService;
        _migrationHandlers = migrationHandlers;
        _usyncConfig = usyncConfig;
    }

    public IEnumerable<string> HandlerTypes()
        => _migrationHandlers
            .OrderBy(x => x.Priority)
            .Select(x => x.ItemType);

    public MigrationResults MigrateFiles(MigrationOptions options)
    {
        var migrationId = Guid.NewGuid();
        var sourceRoot = _migrationFileService.GetMigrationSource("data");

        var itemTypes = options.Handlers.Where(x => x.Include == true).Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var handlers = GetHandlers(itemTypes);

        var migrationContext = PrepareContext(migrationId, sourceRoot, options);

        var results = MigrateFromDisk(migrationId, sourceRoot, migrationContext, handlers);

        var success = results.All(x => x.MessageType == MigrationMessageType.Success);

        if (success == true)
        {
            // if everything works
            _migrationFileService.CopyMigrationToFolder(migrationId, _usyncConfig.GetRootFolder());
        }

        return new MigrationResults
        {
            Success = success,
            MigrationId = migrationId,
            Messages = results
        };
    }

    private IOrderedEnumerable<ISyncMigrationHandler> GetHandlers(HashSet<string>? itemTypes = null)
    {
        if (itemTypes?.Any() == true)
        {
            return _migrationHandlers
                .Where(x => itemTypes.Contains(x.ItemType) == true)
                .OrderBy(x => x.Priority);
        }

        return _migrationHandlers.OrderBy(x => x.Priority);
    }

    private static IEnumerable<MigrationMessage> MigrateFromDisk(Guid migrationId, string sourceRoot, SyncMigrationContext migrationContext, IOrderedEnumerable<ISyncMigrationHandler> handlers)
    {
        // maybe replace with a Dictionary<string, MigrationMessage> (with `ItemType` as the key)?
        var results = new List<MigrationMessage>();

        foreach (var handler in handlers)
        {
            results.AddRange(handler.MigrateFromDisk(migrationId, sourceRoot, migrationContext));
        }

        return results;
    }

    private SyncMigrationContext PrepareContext(Guid migrationId, string sourceRoot, MigrationOptions options)
    {
        var context = new SyncMigrationContext();

        if (options.BlockListViews)
        {
            context.AddBlocked(nameof(DataType), UmbConstants.PropertyEditors.Aliases.ListView);
        }

        if (options.BlockCommonTypes)
        {
            context.AddBlocked(nameof(MediaType), UmbConstants.Conventions.MediaTypes.File);
            context.AddBlocked(nameof(MediaType), UmbConstants.Conventions.MediaTypes.Folder);
            context.AddBlocked(nameof(MediaType), UmbConstants.Conventions.MediaTypes.Image);
        }

        var allHandlers = GetHandlers();

        foreach (var handler in allHandlers)
        {
            handler.PrepareMigrations(migrationId, sourceRoot, context);
        }

        // TODO: Maybe have a notification event to let 3rd-party populate the context? [LK]

        return context;
    }
}
