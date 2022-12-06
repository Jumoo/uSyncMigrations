using System.Reflection;
using System.Xml.Linq;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

using uSync.Core;
using uSync.Migrations.Models;
using uSync.Migrations.Notifications;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;
internal abstract class MigrationHandlerBase<TObject>
    where TObject : IEntity
{
    protected readonly IEventAggregator _eventAggregator;
    protected readonly ISyncMigrationFileService _migrationFileService;

    protected MigrationHandlerBase(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService)
    {
        _eventAggregator = eventAggregator;
        _migrationFileService = migrationFileService;

        var attribute = GetType().GetCustomAttribute<SyncMigrtionHandlerAttribute>();
        if (attribute == null)
        {
            throw new ArgumentException("Handler must has SyncMigrationAttribute to use base class");
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
        var files = GetSourceFiles(context.SourceFolder);
        if (files == null)
        {
            return;
        }

        foreach (var file in files)
        {
            var source = XElement.Load(file);   
            PrepareFile(source, context);
        }
    }

    // for global prepapre stuff. 
    public virtual void Prepare(SyncMigrationContext context) { }

    public virtual IEnumerable<MigrationMessage> DoMigration(SyncMigrationContext context)
    {
        return MigrateFolder(GetSourceFolder(context.SourceFolder), 0, context);
    }

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

        foreach (var file in files)
        {
            var source = XElement.Load(file);

            var migratingNotification = new SyncMigratingNotification<TObject>(source, context);
            if (_eventAggregator.PublishCancelable(migratingNotification) == true)
            {
                continue;
            }

            var target = MigrateFile(source, level, context);

            if (target != null)
            {
                var migratedNotification = new SyncMigratedNotification<TObject>(target, context).WithStateFrom(migratingNotification);
                _eventAggregator.Publish(migratedNotification);
                messages.Add(SaveTargetXml(context.MigrationId, target));
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
    protected abstract XElement? MigrateFile(XElement source, int level, SyncMigrationContext context);
}
