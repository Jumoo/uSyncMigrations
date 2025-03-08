﻿using System.Text;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;

using Newtonsoft.Json;

using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

using uSync.BackOffice;
using uSync.BackOffice.Hubs;
using uSync.Migrations.Core.Helpers;
using uSync.Migrations.Core.Models;
using uSync.Migrations.Core.Plans.CoreProfiles;
using uSync.Migrations.Core.Plans.Models;

namespace uSync.Migrations.Core.Services;

/// <summary>
///  service for reading and writing the status of a migration
/// </summary>
/// <remarks>
///  at the moment we read write a _.status file from disk, but 
///  we could in theory swap this out and use a DB or something
///  if we really wanted to.
/// </remarks>
internal class SyncMigrationStatusService : ISyncMigrationStatusService
{
    private static readonly string _statusFileName = "_.status";
    private static readonly string _siteFolderName = "_site";
    private static readonly string _migrationTargetFolderName = "migrated";
    private static readonly string _migrationsFolderName = "uSync/Migrate";

    private static readonly Dictionary<int, string> _defaultProfiles = new Dictionary<int, string>()
    {
        { 7, "UpgradeUmbracoSevenPlan" },
        { 8, nameof(BlockMigrationPlan) }
    };

    private readonly IShortStringHelper _shortStringHelper;
    private readonly IWebHostEnvironment _webHostEnvironment;

    private readonly IHubContext<SyncHub> _hubContext;

    private readonly string _migrateRoot;

    public SyncMigrationStatusService(
        IWebHostEnvironment webHostEnvironment,
        IShortStringHelper shortStringHelper,
        IHubContext<SyncHub> hubContext)
    {
        _webHostEnvironment = webHostEnvironment;
        _shortStringHelper = shortStringHelper;
        _migrateRoot = _webHostEnvironment.MapPathContentRoot(_migrationsFolderName);
        _hubContext = hubContext;
    }

    public IEnumerable<MigrationStatus> GetAll()
    {
        var migrations = new List<MigrationStatus>();

        if (!Directory.Exists(_migrateRoot)) return migrations;

        foreach (var migrationFolder in Directory.GetDirectories(_migrateRoot))
        {
            var status = LoadStatus(migrationFolder);

            if (status != null)
            {
                migrations.Add(status);
            };
        }

        return migrations;
    }

    public MigrationStatus? Get(string id)
    {
        return GetAll()
            .FirstOrDefault(x => x.Id != null && x.Id.Equals(id));
    }

    public MigrationStatus? LoadStatus(string folder)
    {
        var statusFile = GetStatusFilePath(folder);

        if (File.Exists(statusFile))
        {
            var json = File.ReadAllText(statusFile);
            var status = JsonConvert.DeserializeObject<MigrationStatus>(json);
            if (status != null)
            {
                status.Root = GetSiteRelativePath(folder);

                if (string.IsNullOrEmpty(status.Name))
                {
                    status.Name = Path.GetFileNameWithoutExtension(folder);
                }
                return status;
            }
        }

        return null;
    }

    /// <summary>
    ///  create a new status for the folder. 
    /// </summary>
    public MigrationStatus? CreateStatus(string folder)
    {
        var attempt = MigrationIoHelpers.FinduSyncPath(folder);
        if (attempt.Success && attempt.Result != null)
        {
            var id = Path.GetFileNameWithoutExtension(folder).ToSafeAlias(_shortStringHelper);
            var detectedVersion = MigrationIoHelpers.DetectVersion(attempt.Result);
            var siteFolder = Path.Combine(folder, _siteFolderName);
            var targetFolder = Path.Combine(folder, _migrationTargetFolderName);

            var status = new MigrationStatus
            {
                Root = folder,
                Icon = "icon-folder",
                Id = id,
                Source = GetSiteRelativePath(attempt.Result),
                Version = detectedVersion,
                Target = GetSiteRelativePath(targetFolder),
                Name = id,
                SiteFolder = Directory.Exists(siteFolder) ? GetSiteRelativePath(siteFolder) : null,
                Plan = _defaultProfiles[detectedVersion]
            };

            return status;

        }

        return null;

    }

    public MigrationStatus? CreateNew(MigrationStatus status)
    {
        if (status == null || status.Source == null) return null;

        status.Id = status.Name?.ToSafeFileName(_shortStringHelper) ??
            Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
        status.Version = MigrationIoHelpers.DetectVersion(status.Source);
        status.Root = Path.Combine(_migrateRoot, status.Id);

        if (status.Icon == null)
        {
            status.Icon = "icon-folder";
        }

        return status;
    }

    public string? GetDefaultProfile(int version)
    {
        if (_defaultProfiles.ContainsKey(version))
            return _defaultProfiles[version];

        return null;
    }

    public void SaveStatus(string? folder, MigrationStatus status)
    {
        if (string.IsNullOrWhiteSpace(folder)) return;

        var statusFile = GetStatusFilePath(folder);
        if (File.Exists(statusFile))
            File.Delete(statusFile);

        Directory.CreateDirectory(folder);

        var json = JsonConvert.SerializeObject(status, Formatting.Indented);
        File.WriteAllText(statusFile, json, Encoding.UTF8);
    }

    private static string GetStatusFilePath(string folder)
        => Path.Combine(folder, _statusFileName);

    private string GetSiteRelativePath(string folder)
        => folder.Substring(_webHostEnvironment.ContentRootPath.Length + 1);

    public MigrationOptions? ConvertToOptions(MigrationStatus status, MigrationOptions defaultOptions)
    {
        if (status.Source == null || status.Target == null) return null;

        uSyncCallbacks callbacks = null;
        if (!string.IsNullOrEmpty(status.ClientId))
        {
            var client = new HubClientService(_hubContext, status.ClientId);
            callbacks = client.Callbacks();
        }

        return new MigrationOptions
        {
            Source = status.Source,
            Target = status.Target,
            SiteFolder = status.SiteFolder ?? "/",
            SourceVersion = status.Version,
            BlockCommonTypes = defaultOptions.BlockCommonTypes,
            BlockedItems = defaultOptions.BlockedItems,
            BlockListViews = defaultOptions.BlockListViews,
            ChangeTabs = defaultOptions.ChangeTabs,
            Group = defaultOptions.Group,
            Handlers = defaultOptions.Handlers,
            IgnoredProperties = defaultOptions.IgnoredProperties,
            IgnoredPropertiesByContentType = defaultOptions.IgnoredPropertiesByContentType,
            MigrationType = defaultOptions.MigrationType,
            PreferredMigrators = defaultOptions.PreferredMigrators,
            PropertyMigrators = defaultOptions.PropertyMigrators,
            MergingProperties = defaultOptions.MergingProperties,
            ReplacementAliases = defaultOptions.ReplacementAliases,
            Callbacks = callbacks ?? null,
        };
    }
}
