using System.Diagnostics;

using Microsoft.Extensions.Logging;

using NUglify.Helpers;

using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

using uSync.BackOffice.Configuration;
using uSync.Migrations.Composing;
using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Context;
using uSync.Migrations.Handlers;
using uSync.Migrations.Helpers;
using uSync.Migrations.Migrators.Community.Archetype;
using uSync.Migrations.Models;

namespace uSync.Migrations.Services;

internal class SyncMigrationService : ISyncMigrationService
{
    private readonly ISyncMigrationFileService _migrationFileService;
    private readonly ILogger<SyncMigrationService> _logger;

    private readonly SyncMigrationHandlerCollection _migrationHandlers;
    private readonly SyncMigrationValidatorCollection _migrationValidators;
    private readonly uSyncConfigService _usyncConfig;
    private readonly SyncPropertyMigratorCollection _migrators;
    private readonly ArchetypeMigrationConfigurerCollection _archetypeConfigurers;
    private readonly SyncPropertyMergingCollection _mergingCollection;

    public SyncMigrationService(
        ILogger<SyncMigrationService> logger,
        ISyncMigrationFileService migrationFileService,
        SyncMigrationHandlerCollection migrationHandlers,
        uSyncConfigService usyncConfig,
        SyncMigrationValidatorCollection migrationValidators,
        SyncPropertyMigratorCollection migrators,
        ArchetypeMigrationConfigurerCollection archetypeConfigures,
        SyncPropertyMergingCollection mergingCollection)
    {
        _logger = logger;

        _migrationFileService = migrationFileService;
        _migrationHandlers = migrationHandlers;
        _usyncConfig = usyncConfig;
        _migrationValidators = migrationValidators;
        _migrators = migrators;
        _archetypeConfigurers = archetypeConfigures;
        _mergingCollection = mergingCollection;
    }

    public IEnumerable<string> HandlerTypes(int version)
        => _migrationHandlers
            .Where(x => x.SourceVersion == version)
            .OrderBy(x => x.Priority)
            .Select(x => x.ItemType);

    public IEnumerable<ISyncMigrationHandler> GetHandlers(int version)
        => _migrationHandlers.Where(x => x.SourceVersion == version);


    public int DetectVersion(string folder)
    {
        var uSyncFolder = _migrationFileService.GetMigrationFolder(folder, false);
        return MigrationIoHelpers.DetectVersion(uSyncFolder);   
    }

     /// <summary>
    ///  validate things before we run through them and do an actuall migration.
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

        var messages = new List<MigrationMessage>();

        foreach(var validator in _migrationValidators)
        {
            try
            {
                messages.AddRange(validator.Validate(options));
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

        var handlers = GetHandlers(options.SourceVersion, itemTypes);

        using (var migrationContext = PrepareContext(migrationId, sourceRoot, options))
        {
            var results = MigrateFromDisk(migrationId, sourceRoot, migrationContext, handlers);

            var success = results.All(x => x.MessageType != MigrationMessageType.Error);

            if (success == true && results.Any())
            {
                // here...
                _logger.LogInformation("Copying from working to folder {targetRoot}", targetRoot);

                // if everything works
                _migrationFileService.CopyMigrationToFolder(migrationId, targetRoot);
                _migrationFileService.RemoveMigration(migrationId);
            }

            sw.Stop();
            _logger.LogInformation("Migration Complete {success} {count} ({elapsed}ms)", success, results.Count(), sw.ElapsedMilliseconds);

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
            return _migrationHandlers
                .Where(x => x.SourceVersion == sourceVersion && itemTypes.Contains(x.ItemType) == true)
                .OrderBy(x => x.Priority);
        }

        return _migrationHandlers
            .Where(x => x.SourceVersion == sourceVersion)
            .OrderBy(x => x.Priority);
    }

    private IEnumerable<MigrationMessage> MigrateFromDisk(Guid migrationId, string sourceRoot, SyncMigrationContext migrationContext, IOrderedEnumerable<ISyncMigrationHandler> handlers)
    {
        // maybe replace with a Dictionary<string, MigrationMessage> (with `ItemType` as the key)?
        var results = new List<MigrationMessage>();

        foreach (var handler in handlers)
        {
            _logger.LogInformation("Migrating {handler} files", handler.GetType().Name);
            results.AddRange(handler.DoMigration(migrationContext));
        }

        return results;
    }

    private SyncMigrationContext PrepareContext(Guid migrationId, string sourceRoot, MigrationOptions options)
    {
        _logger.LogInformation("PrepareContext {id} {source}", migrationId, sourceRoot);

        var context = new SyncMigrationContext(migrationId, sourceRoot, options.SourceVersion);

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

        AddMigrators(context, options.PreferredMigrators);

        AddMergers(context, options.MergingProperties);

        // let the handlers run through their prep (populate all the lookups)
        GetHandlers(options.SourceVersion)?
            .OrderBy(x => x.Priority)
            .ToList()
            .ForEach(x => x.PrepareMigrations(context));

        // add configurer for Archetype migrations
        context.ContentTypes.ArchetypeMigrationConfigurer = _archetypeConfigurers.FirstOrDefault(c => c.GetType().Name == options.ArchetypeMigrationConfigurer) 
            ?? new DefaultArchetypeMigrationConfigurer();

        return context;
    }

    private void AddMigrators(SyncMigrationContext context, IDictionary<string,string>? preferredMigrators)
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
