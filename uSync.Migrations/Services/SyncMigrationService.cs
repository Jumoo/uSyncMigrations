using NUglify.Helpers;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

using uSync.BackOffice.Configuration;
using uSync.Migrations.Composing;
using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Handlers;
using uSync.Migrations.Models;

namespace uSync.Migrations.Services;

internal class SyncMigrationService : ISyncMigrationService
{
    private readonly ISyncMigrationFileService _migrationFileService;
    private readonly SyncMigrationHandlerCollection _migrationHandlers;
    private readonly SyncMigrationValidatorCollection _migrationValidators;
    private readonly uSyncConfigService _usyncConfig;
    private readonly SyncPropertyMigratorCollection _migrators;

    public SyncMigrationService(
        ISyncMigrationFileService migrationFileService,
        SyncMigrationHandlerCollection migrationHandlers,
        uSyncConfigService usyncConfig,
        SyncMigrationValidatorCollection migrationValidators,
        SyncPropertyMigratorCollection migrators)
    {
        _migrationFileService = migrationFileService;
        _migrationHandlers = migrationHandlers;
        _usyncConfig = usyncConfig;
        _migrationValidators = migrationValidators;
        _migrators = migrators;
    }

    public IEnumerable<string> HandlerTypes()
        => _migrationHandlers
            .OrderBy(x => x.Priority)
            .Select(x => x.ItemType);

    public IEnumerable<ISyncMigrationHandler> GetHandlers()
        => _migrationHandlers;

    public Attempt<string> ValidateMigrationSource(string source)   
        => _migrationFileService.ValdateMigrationSource(source);

    /// <summary>
    ///  validate things before we run through them and do an actuall migration.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public MigrationResults Validate(MigrationOptions options)
    {
        options.Source = _migrationFileService.GetMigrationFolder(options.Source);

        var messages = new List<MigrationMessage>();

        foreach(var validator in _migrationValidators)
        {
            try
            {
                messages.AddRange(validator.Validate(options));
            }
            catch(Exception ex)
            {
                // TODO: what do we do if the validator fails ???
            }
        }

        return new MigrationResults
        {
            Messages = messages,
            MigrationId = Guid.Empty,
            Success = !messages.Any(x => x.MessageType == MigrationMessageType.Error)
        };
    }

    public MigrationResults MigrateFiles(MigrationOptions options)
    {
        var migrationId = Guid.NewGuid();
        var sourceRoot = _migrationFileService.GetMigrationFolder(options.Source);
        var targetRoot = _migrationFileService.GetMigrationFolder(options.Target);

        // TODO: Add notifications for `uSyncMigrationStartingNotification` and `uSyncMigrationCompleteNotification`? [LK]
        // Pass through the context, in case 3rd-party wants to populate/reference it? [LK]

        var itemTypes = options.Handlers.Where(x => x.Include == true).Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var handlers = GetHandlers(itemTypes);

        var migrationContext = PrepareContext(migrationId, sourceRoot, options);

        var results = MigrateFromDisk(migrationId, sourceRoot, migrationContext, handlers);

        var success = results.All(x => x.MessageType != MigrationMessageType.Error);

        if (success == true && results.Count() > 0)
        {
            // if everything works
            _migrationFileService.CopyMigrationToFolder(migrationId, targetRoot);
            _migrationFileService.RemoveMigration(migrationId);
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
        var context = new SyncMigrationContext(migrationId);

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

        // items that we block by type and alias 
        options.BlockedItems?
            .ForEach(kvp => kvp.Value?
                .ForEach(value => context.AddBlocked(kvp.Key, value)));

        // properties we ignore globally.
        options.IgnoredProperties?
            .ForEach(x => context.AddIgnoredProperty(x));

        // properties we ignore by content type
        options.IgnoredPropertiesByContentType?
            .ForEach(kvp =>
                kvp.Value?.ForEach(value => context.AddIgnoredProperty(kvp.Key, value)));

        // load the migrators into the context - if they are defined in options
        // they can be overridden. 
        var migrators = options.Migrators ?? _migrators.GetDefaultMigrators();
        migrators.ForEach(x => context.AddMigrator(x));

        // let the handlers run through their prep (populate all the lookups)
        GetHandlers()?
            .OrderBy(x => x.Priority)
            .ToList()
            .ForEach(x => x.PrepareMigrations(migrationId, sourceRoot, context));

        return context;
    }
}
