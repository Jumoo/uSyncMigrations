using Umbraco.Extensions;

using uSync.BackOffice.Configuration;
using uSync.Migrations.Handlers;
using uSync.Migrations.Models;

namespace uSync.Migrations.Services;
public class MigrationService 
{
    private readonly MigrationHandlerCollection _migrationHandlers;
   
    private readonly uSyncConfigService _uSyncConfig;
    private readonly MigrationFileService _migrationFileService;

    public MigrationService(
        MigrationHandlerCollection migrationHandlers,
        uSyncConfigService uSyncConfig,
        MigrationFileService migrationFileService)
    {
        _migrationHandlers = migrationHandlers;
        _uSyncConfig = uSyncConfig;
        _migrationFileService = migrationFileService;
    }

    public IEnumerable<string> HandlerTypes()
        => _migrationHandlers
            .OrderBy(x => x.Priority)
            .Select(x => x.ItemType) ;

    public MigrationResults MigrateFiles(MigrationOptions options)
    {
        var migrationId = Guid.NewGuid();
        var sourceRoot = _migrationFileService.GetMigrationSource("data");
        var migrationRoot = Path.Combine(sourceRoot, migrationId.ToString());

        var itemTypes = options.Handlers.Where(x => x.Include).Select(x => x.Name);

        IOrderedEnumerable<ISyncMigrationHandler> handlers = GetHandlers(itemTypes);

        var migrationContext = PrepContext(migrationId, sourceRoot, options);

        var results = MigrateFromDisk(migrationId, sourceRoot, migrationContext, handlers);
        var success = results.Count() > 0 && results.All(x => x.MessageType == MigrationMessageType.Success);

        if (success)
        {
            // if everything works
            _migrationFileService.CopyMigrationToFolder(migrationId,
                _uSyncConfig.GetRootFolder());
        }

        return new MigrationResults
        {
            Success = success,
            MigrationId = migrationId,
            Messages = results
        };        
    }

    private IOrderedEnumerable<ISyncMigrationHandler> GetHandlers(IEnumerable<string> itemTypes)
    {
        if (itemTypes != null && itemTypes.Count() > 0)
        {
            return _migrationHandlers
                .Where(x => itemTypes.Contains(x.ItemType))
                .OrderBy(x => x.Priority);
        }

        return _migrationHandlers.OrderBy(x => x.Priority);
    }

    private static IEnumerable<MigrationMessage> MigrateFromDisk(Guid migrationId, string sourceRoot, MigrationContext migrationContext, IOrderedEnumerable<ISyncMigrationHandler> handlers)
    {
        var results = new List<MigrationMessage>();

        foreach (var handler in handlers)
        {
            results.AddRange(handler.MigrateFromDisk(migrationId, sourceRoot, migrationContext));
        }

        return results;
    }


    private MigrationContext PrepContext(Guid migrationId, string root, MigrationOptions options)
    {
        var context = new MigrationContext();

        if (options.BlockListViews)
            context.AddBlocked("DataType", "Umbraco.ListView");

        if (options.BlockCommonTypes)
        {
            context.AddBlocked("MediaType", "File");
            context.AddBlocked("MediaType", "Folder");
            context.AddBlocked("MediaType", "Image");
        }

        var allHandlers = GetHandlers(Enumerable.Empty<string>());

        foreach (var handler in allHandlers)
            handler.PrepMigrations(migrationId, root, context);

        return context;
    }
}
