using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;

using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

using uSync.Core;
using uSync.Migrations.Core.Context;
using uSync.Migrations.Core.Models;
using uSync.Migrations.Core.Notifications;
using uSync.Migrations.Core.Services;

namespace uSync.Migrations.Core.Handlers;
internal abstract class MigrationHandlerBase<TObject>
    where TObject : IEntity
{
    protected readonly IEventAggregator _eventAggregator;
    protected readonly ISyncMigrationFileService _migrationFileService;
    protected readonly ILogger<MigrationHandlerBase<TObject>> _logger;

    protected MigrationHandlerBase(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        ILogger<MigrationHandlerBase<TObject>> logger)
    {
        _eventAggregator = eventAggregator;
        _migrationFileService = migrationFileService;
        _logger = logger;

        var attribute = GetType().GetCustomAttribute<SyncMigrationHandlerAttribute>();
        if (attribute == null)
        {
            throw new ArgumentException("Handler must have SyncMigrationAttribute to use base class");
        }

        ItemType = typeof(TObject).Name;
        Priority = attribute.Priority;
        SourceVersion = attribute.SourceVersion;
        Group = attribute.Group;

        if (!string.IsNullOrWhiteSpace(attribute.SourceFolderName))
        {
            SourceFolderName = attribute.SourceFolderName;
        }

        if (!string.IsNullOrWhiteSpace(attribute.TargetFolderName))
        {
            DestinationFolderName = attribute.TargetFolderName;
        }
    }

    public string Group { get; protected set; }
    public int Priority { get; protected set; }
    public int SourceVersion { get; protected set; }
    public string ItemType { get; protected set; }

    protected string? SourceFolderName { get; init; }
    protected string? DestinationFolderName { get; init; }

    protected virtual string GetSourceFolder(string sourceRoot)
        => Path.Combine(sourceRoot, SourceFolderName ?? string.Empty);

    protected virtual List<string>? GetSourceFiles(string sourceRoot)
    {
        var folder = GetSourceFolder(sourceRoot);

        if (Directory.Exists(folder) == false)
        {
            return null;
        }

        return Directory
            .GetFiles(folder, "*.config", SearchOption.AllDirectories)
            .ToList();
    }

    public virtual void PrepareMigrations(SyncMigrationContext context)
    {
        Stopwatch sw = Stopwatch.StartNew();

        var typeName = typeof(TObject).Name;

        context.SendUpdate($"Preparing : {typeName}", 0, 0);

        _logger.LogInformation("[{type}] Preparing Migration {source}", typeName, context.Metadata.SourceFolder);
        var files = GetSourceFiles(context.Metadata.SourceFolder);
        if (files == null)
        {
            return;
        }

        // load anything handler specific. 
        Prepare(context);

        // loop through the files
        List<XElement> nodes = files.Select(XElement.Load).ToList();
        nodes.ForEach(x => PrePrepareFile(x, context));
        nodes.ForEach(x => PrepareFile(x, context));


        sw.Stop();
        _logger.LogInformation("[{type}] Migration Prep completed ({elapsed}ms)", typeof(TObject).Name, sw.ElapsedMilliseconds);
    }

    // for global prepare stuff. 
    public virtual void Prepare(SyncMigrationContext context) { }

    public virtual IEnumerable<MigrationMessage> DoMigration(SyncMigrationContext context)
    {
        var messages = new List<MigrationMessage>();
        messages.AddRange(PreDoMigration(context));
        messages.AddRange(MigrateFolder(GetSourceFolder(context.Metadata.SourceFolder), 0, context));
        messages.AddRange(PostDoMigration(context));
        return messages;
    }

    /// <summary>
    ///  so handlers can run things before main DoMigrationLoop
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    protected virtual IEnumerable<MigrationMessage> PreDoMigration(SyncMigrationContext context)
        => Enumerable.Empty<MigrationMessage>();

    /// <summary>
    ///  So Handlers can run things post main DoMigrationLoop
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    protected virtual IEnumerable<MigrationMessage> PostDoMigration(SyncMigrationContext context)
        => Enumerable.Empty<MigrationMessage>();

    /// <summary>
    ///  migrate a folder 
    /// </summary>
    /// <remarks>
    ///  We have to do it like this for v7 because it did level by folder structure.
    /// </remarks>
    private IEnumerable<MigrationMessage> MigrateFolder(string folder, int level, SyncMigrationContext context)
    {
        if (Directory.Exists(folder) == false)
        {
            return Enumerable.Empty<MigrationMessage>();
        }

        var files = Directory.GetFiles(folder, "*.config");
        if (files == null)
        {
            return Enumerable.Empty<MigrationMessage>();
        }

        var messages = new List<MigrationMessage>();


        foreach(var file in files) 
        {
            try
            {
                var source = XElement.Load(file);

                // if the file is a delete/rename/etc skip over it. 
                if (source.IsEmptyItem()) continue;

                var (alias, key) = GetAliasAndKey(source, context);
                if (context.IsBlocked(ItemType, alias)) continue;

                var migratingNotification = new SyncMigratingNotification<TObject>(source, context);
                if (_eventAggregator.PublishCancelable(migratingNotification) == true)
                {
                    continue;
                }

                context.SendUpdate($"Migrating {alias}", 0, 0);

                var target = MigrateFile(source, level, context);

                if (target != null)
                {
                    var migratedNotification = new SyncMigratedNotification<TObject>(target, context).WithStateFrom(migratingNotification);
                    _eventAggregator.Publish(migratedNotification);
                    messages.Add(SaveTargetXml(context.Metadata.MigrationId, target));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing {file}", file);

                var name = Path.Combine(
                    Path.GetFileNameWithoutExtension(Path.GetDirectoryName(file)) ?? "",
                    Path.GetFileNameWithoutExtension(file));

                // throw new Exception($"Error processing {file}", ex);
                messages.Add(new MigrationMessage(ItemType, name, MigrationMessageType.Error)
                {
                    Message = ex.Message
                });
            }
        }

        foreach (var childFolder in Directory.GetDirectories(folder))
        {
            messages.AddRange(MigrateFolder(childFolder, level + 1, context));
        }

        return messages;
    }

    protected virtual MigrationMessage SaveTargetXml(Guid id, XElement xml)
    {
        _migrationFileService.SaveMigrationFile(id, xml, DestinationFolderName ?? string.Empty);
        return new MigrationMessage(ItemType, xml.GetAlias(), MigrationMessageType.Success);
    }

    protected abstract void PrepareFile(XElement source, SyncMigrationContext context);
    protected virtual void PrePrepareFile(XElement source, SyncMigrationContext context) { }
    protected abstract XElement? MigrateFile(XElement source, int level, SyncMigrationContext context);

    /// <summary>
    ///  method to get the source and alias of a value.
    /// </summary>
    /// <param name="source">XMLElement for item</param>
    /// <param name="context">Migration context</param>
    /// <returns></returns>
    protected abstract (string alias, Guid key) GetAliasAndKey(XElement source, SyncMigrationContext? context);

    /// <summary>
    ///  Get the Alias and Key values for an item.
    /// </summary>
    /// <remarks>
    ///  this method is obsolete, you should pass the context.
    ///  this then allows for renames, and maniupulation based on config.
    /// </remarks>
    /// <param name="source">XML Source for item</param>
    /// <returns></returns>
    [Obsolete("Call GetAliasAndKey with MigrationContext")]
    protected virtual (string alias, Guid key) GetAliasAndKey(XElement source)
    => GetAliasAndKey(source, null);

}
