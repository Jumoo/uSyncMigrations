using System.Diagnostics;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NUglify.Helpers;

using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

using uSync.BackOffice.Configuration;
using uSync.Migrations.Core.Composing;
using uSync.Migrations.Core.Configuration;
using uSync.Migrations.Core.Configuration.Models;
using uSync.Migrations.Core.Context;
using uSync.Migrations.Core.Handlers;
using uSync.Migrations.Core.Helpers;
using uSync.Migrations.Core.Models;

namespace uSync.Migrations.Core.Services;

internal class SyncMigrationService : ISyncMigrationService
{
    private readonly ISyncMigrationFileService _migrationFileService;
    private readonly IOptions<uSyncMigrationOptions> _options;
    private readonly ILogger<SyncMigrationService> _logger;

    private readonly SyncMigrationHandlerCollection _migrationHandlers;
    private readonly SyncMigrationValidatorCollection _migrationValidators;
    private readonly uSyncConfigService _uSyncConfig;
    private readonly SyncPropertyMigratorCollection _migrators;
    private readonly SyncPropertyMergingCollection _mergingCollection;

    public SyncMigrationService(
        IOptions<uSyncMigrationOptions> options,
        ILogger<SyncMigrationService> logger,
        ISyncMigrationFileService migrationFileService,
        SyncMigrationHandlerCollection migrationHandlers,
        uSyncConfigService uSyncConfig,
        SyncMigrationValidatorCollection migrationValidators,
        SyncPropertyMigratorCollection migrators,
        SyncPropertyMergingCollection mergingCollection)
    {
        _options = options;
        _logger = logger;

        _migrationFileService = migrationFileService;
        _migrationHandlers = migrationHandlers;
        _uSyncConfig = uSyncConfig;
        _migrationValidators = migrationValidators;
        _migrators = migrators;
        _mergingCollection = mergingCollection;
    }

    public IEnumerable<string> HandlerTypes(int version)
        => _migrationHandlers
            .Where(x => HandlerMatches(x, _options.Value, version))
            .OrderBy(x => x.Priority)
            .Select(x => x.ItemType);
    private static bool HandlerMatches(ISyncMigrationHandler handler, uSyncMigrationOptions options, int version)
    {
        if (handler.SourceVersion != version) return false;
        var result = options.DisabledHandlers.Any(x => handler.GetType().Name.InvariantEquals(x)) == false;
        return result;
    }

    public IEnumerable<ISyncMigrationHandler> GetHandlers(int version)
        => _migrationHandlers.Where(x => HandlerMatches(x, _options.Value, version));

    public int DetectVersion(string folder)
    {
        var uSyncFolder = _migrationFileService.GetMigrationFolder(folder, false);
        return MigrationIoHelpers.DetectVersion(uSyncFolder);
    }

    /// <summary>
    ///  validate things before we run through them and do an actual migration.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public MigrationResults Validate(MigrationOptions? options)
    {
        if (options == null)
        {
            return new MigrationResults
            {
                MigrationId = Guid.Empty,
                Success = false,
                Messages = new MigrationMessage("Fail", "No Options", MigrationMessageType.Error).AsEnumerableOfOne()
            };
        }

        options.Source = _migrationFileService.GetMigrationFolder(options.Source, false);
        options.SourceVersion = MigrationIoHelpers.DetectVersion(options.Source);

        var siteFolder = _migrationFileService.GetWebSitePath(options.SiteFolder);
        var siteFolderIsSameAsWebsite = siteFolder.Equals(_migrationFileService.GetWebSitePath("/"));
        var validationContext = new SyncValidationContext(options, Guid.Empty, options.Source, siteFolder, siteFolderIsSameAsWebsite, options.SourceVersion);

        var messages = new List<MigrationMessage>();

        foreach (var validator in _migrationValidators)
        {
            try
            {
                messages.AddRange(validator.Validate(validationContext));
            }
            catch
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
        var sw = Stopwatch.StartNew();

        var migrationId = Guid.NewGuid();
        var sourceRoot = _migrationFileService.GetMigrationFolder(options.Source, false);
        var targetRoot = _migrationFileService.GetMigrationFolder(options.Target, true);

        // make sure its here.
        _logger.LogInformation("Migrating from {source} to {target}", sourceRoot, targetRoot);

        // TODO: Add notifications for `uSyncMigrationStartingNotification` and `uSyncMigrationCompleteNotification`? [LK]
        // Pass through the context, in case 3rd-party wants to populate/reference it? [LK]

        var itemTypes = options.Handlers?.Where(x => x.Include == true).Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var handlers = GetHandlers(options.SourceVersion, itemTypes).ToList();

        using (var migrationContext = PrepareContext(migrationId, sourceRoot, options))
        {
            var results = MigrateFromDisk(migrationId, sourceRoot, migrationContext, handlers);

            var success = results.All(x => x.MessageType != MigrationMessageType.Error);

            if (success == true && results.Any())
            {
                migrationContext.SendUpdate("Copying to final folder", 9, 10);

                // here...
                _logger.LogInformation("Copying from working to folder {targetRoot}", targetRoot);

                // if everything works
                _migrationFileService.CopyMigrationToFolder(migrationId, targetRoot);
                _migrationFileService.RemoveMigration(migrationId);
            }

            sw.Stop();
            _logger.LogInformation("Migration Complete {success} {count} ({elapsed}ms)", success, results.Count(), sw.ElapsedMilliseconds);

            migrationContext.SendUpdate("Migration complete", 10, 10);

            return new MigrationResults
            {
                Success = success,
                MigrationId = migrationId,
                Messages = results
            };
        }
    }

    private IOrderedEnumerable<ISyncMigrationHandler> GetHandlers(int sourceVersion, HashSet<string>? itemTypes = null)
    {
        if (itemTypes?.Any() == true)
        {
            return _migrationHandlers.Where(x => HandlerMatches(x, _options.Value, sourceVersion))
                .Where(x => itemTypes.Contains(x.ItemType) == true)
                .OrderBy(x => x.Priority);
        }

        return _migrationHandlers.Where(x => HandlerMatches(x, _options.Value, sourceVersion))
            .OrderBy(x => x.Priority);
    }

    private IEnumerable<MigrationMessage> MigrateFromDisk(Guid migrationId, string sourceRoot, SyncMigrationContext migrationContext, IList<ISyncMigrationHandler> handlers)
    {
        // maybe replace with a Dictionary<string, MigrationMessage> (with `ItemType` as the key)?
        var results = new List<MigrationMessage>();

        for(int i =0; i < handlers.Count; i++)
        {
            var handlerName = handlers[i].GetType().Name;   
            
            migrationContext.SendUpdate($"Migrating {handlerName}", i, handlers.Count);
            _logger.LogInformation("Migrating {handler} files", handlerName);
            
            results.AddRange(handlers[i].DoMigration(migrationContext));
        }

        return results;
    }

    private SyncMigrationContext PrepareContext(Guid migrationId, string sourceRoot, MigrationOptions options)
    {
        _logger.LogInformation("PrepareContext {id} {source}", migrationId, sourceRoot);

        var siteFolder = _migrationFileService.GetWebSitePath(options.SiteFolder);
        var siteFolderIsSameAsWebsite = siteFolder.Equals(_migrationFileService.GetWebSitePath("/"));

        var context = new SyncMigrationContext(migrationId, sourceRoot, siteFolder, siteFolderIsSameAsWebsite, options.SourceVersion);
        context.Callbacks = options.Callbacks;

        context.SendUpdate("Preparing Migration", 0, 10);

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
            .ForEach(x => context.ContentTypes.AddIgnoredProperty(x));

        // properties we ignore by content type
        options.IgnoredPropertiesByContentType?
            .ForEach(kvp =>
                kvp.Value?.ForEach(value => context.ContentTypes.AddIgnoredProperty(kvp.Key, value)));

        // tabs that we might want to rename, delete or move properties from/to
        options.ChangeTabs?
            .ForEach(x => context.ContentTypes.AddChangedTabs(x));

        AddPropertyMigrators(context, options.PropertyMigrators);

        AddMigrators(context, options.PreferredMigrators);

        AddMergers(context, options.MergingProperties);

        options.ReplacementAliases?
            .ForEach(kvp => context.ContentTypes.AddReplacementAlias(kvp.Key, kvp.Value));

        context.SendUpdate("Preparing Migration handlers", 5,10);

        // let the handlers run through their prep (populate all the lookups)
        GetHandlers(options.SourceVersion)?
            .OrderBy(x => x.Priority)
            .ToList()
            .ForEach(x => x.PrepareMigrations(context));

        return context;
    }


    private void AddPropertyMigrators(SyncMigrationContext context, IDictionary<string, string>? propertyMigrators)
    {
        if (propertyMigrators?.Count > 0)
        {
            foreach (var item in propertyMigrators)
            {
                var migrator = _migrators.GetMigrator(item.Value);
                if (migrator is not null)
                {
                    context.Migrators.AddPropertyAliasMigration(item.Key, migrator);
                }
            }
        }
    }

    private void AddMigrators(SyncMigrationContext context, IDictionary<string, string>? preferredMigrators)
    {
        _logger.LogInformation("Adding migrators");

        var preferredList = _migrators.GetPreferredMigratorList(preferredMigrators);
        if (preferredList != null)
        {
            foreach (var item in preferredList.Where(x => x.Migrator.Versions.Contains(context.Metadata.SourceVersion)))
            {
                context.Migrators.AddPropertyMigration(item.EditorAlias, item.Migrator);
            }
        }
    }

    private void AddMergers(SyncMigrationContext context, Dictionary<string, MergingPropertiesConfig> mergingProperties)
    {
        _logger.LogInformation("Adding property mergers");

        foreach (var mergingProperty in mergingProperties)
        {
            // find the merger. 
            var merger = _mergingCollection.GetByName(mergingProperty.Value.Merger);
            if (merger == null) continue;

            // add the merger to the context. 
            _logger.LogInformation("Loading Merger {merger} for {contentType}", merger.GetType().Name, mergingProperty.Key);
            context.Migrators.AddMergingMigrator(mergingProperty.Key, merger);

            // the merging properties. 
            context.Content.AddMergedProperty(mergingProperty.Key, mergingProperty.Value);
        }
    }
}
