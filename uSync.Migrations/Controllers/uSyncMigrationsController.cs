using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Extensions;

using uSync.BackOffice;
using uSync.BackOffice.Services;
using uSync.Migrations.Configuration;
using uSync.Migrations.Configuration.CoreProfiles;
using uSync.Migrations.Configuration.Models;
using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Controllers;


[Authorize(Policy = uSyncMigrationsAuthorizationPolicies.MigrationsTreeAccess)]
public class uSyncMigrationsController : UmbracoAuthorizedApiController
{
    private readonly IShortStringHelper _shortStringHelper;

    private readonly ISyncMigrationService _migrationService;
    private readonly ISyncMigrationFileService _migrationFileService;
    private readonly ISyncMigrationConfigurationService _profileConfigService;
    private readonly ISyncMigrationStatusService _migrationStatusService;

    private readonly uSyncService _uSyncService;
    private readonly SyncFileService _syncFileService;
    private readonly string _tempPath;
    private readonly string _siteRoot;

    public uSyncMigrationsController(
        ISyncMigrationService migrationService,
        ISyncMigrationConfigurationService profileConfigService,
        IWebHostEnvironment webHostEnvironment,
        uSyncService uSyncService,
        SyncFileService syncFileService,
        ISyncMigrationFileService migrationFileService,
        IShortStringHelper shortStringHelper,
        ISyncMigrationStatusService migrationStatusService)
    {
        _migrationService = migrationService;
        _profileConfigService = profileConfigService;
        _uSyncService = uSyncService;

        _tempPath = Path.GetFullPath(
            Path.Combine(webHostEnvironment.ContentRootPath, "uSync", "migrate"));
        _siteRoot = Path.GetFullPath(webHostEnvironment.ContentRootPath);

        _syncFileService = syncFileService;
        _migrationFileService = migrationFileService;
        _shortStringHelper = shortStringHelper;
        _migrationStatusService = migrationStatusService;
    }

    [HttpGet]
    public bool GetApi() => true;


    [HttpPost]
    [Authorize(Roles = UmbConstants.Security.AdminGroupAlias)]
    public async Task<UploadResult> Upload()
    {
        var file = Request.Form.Files[0];

        if (file.Length > 0)
        {
            var tempFile = GetSafeTempFileName(file.FileName);

            using (var stream = new FileStream(tempFile, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var tempFolder = Path.Combine(_tempPath,
                Path.GetFileNameWithoutExtension(tempFile));

            try
            {
                _uSyncService.DeCompressFile(tempFile, tempFolder);

                var status = _migrationStatusService.CreateStatus(tempFolder);
                if (status != null)
                {
                    status.Icon = "icon-zip";
                    _migrationStatusService.SaveStatus(tempFolder, status);
                }

                return new UploadResult
                {
                    Success = true,
                    Status = status
                };
            }
            catch { throw; }
            finally
            {
                // clean up ?
                _syncFileService.DeleteFile(tempFile);
            }
        }
        throw new Exception("Unsupported");
    }

    private string GetSafeTempFileName(string filename)
    {
        var safeFileName = Path.GetFileNameWithoutExtension(filename)
               .ToSafeFileName(_shortStringHelper) + ".zip";

        Directory.CreateDirectory(_tempPath);
        return Path.Combine(_tempPath, safeFileName);
    }


    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class UploadResult
    {
        public bool Success { get; init; }

        public MigrationStatus? Status { get; init; }

    }

    /// <summary>
    ///  looks to see if there is a usync/data folder, and if there is
    ///  if we have migrated it in the past.
    /// </summary>
    [HttpGet]
    public bool HasPendingMigration() => true;

    [HttpGet]
    public object GetMigrationOptions(int version)
        => new {
            hasPending = true,
            handlers = _migrationService.HandlerTypes(version).Select(x => new HandlerOption { Name = x, Include = false })
        };

    [HttpPost]
    public MigrationResults? Migrate(MigrationStatus status)
    {
        if (status?.Plan == null) return null;

        var profile = _profileConfigService.GetPlan(status.Plan);
        if (profile == null) return null;

        var options = _migrationStatusService.ConvertToOptions(status, profile.Options);

        if (options == null) return null;
        var results = _migrationService.MigrateFiles(options);

        if (results.Success)
        {
            status.Migrated = true;
            _migrationStatusService.SaveStatus(status.Root, status);
        }

        return results;
    }

    [HttpGet]
    public IEnumerable<ISyncMigrationPlan> GetProfiles(string groupAlias)
        => _profileConfigService.GetPlans(groupAlias);

    [HttpGet]
    public IDictionary<string, string> GetPreferedMigrators(string planName)
    {
        var plan = _profileConfigService.GetPlan(planName);
        return plan?.Options?.PreferredMigrators ?? new Dictionary<string, string>();
    }

    [HttpGet]
    public Dictionary<string, string> GetProfilesByVersion(int version)
        => _profileConfigService.GetPlans()
            .Plans.Where(x => x.Options.SourceVersion == version)
            .ToDictionary(x => x.GetType().Name, y => y.Name);

    [HttpGet]
    public string GetDefaultProfile(int version)
        => _migrationStatusService.GetDefaultProfile(version) ?? nameof(UpgradeUmbracoSevenPlan);

    [HttpGet]
    public int DetectVersion(string folder)
        => _migrationService.DetectVersion(folder);

    [HttpGet]
    public string GetDefaultTarget(int version)
    {
        var profileName = _migrationStatusService.GetDefaultProfile(version);
        var defaultProfile = _profileConfigService.GetPlan(profileName ?? nameof(UpgradeUmbracoSevenPlan));
        return defaultProfile?.Options.Target ?? "";
    }

    [HttpGet]
    public MigrationStatus GetConversionDefaults()
    {
        var profileName = _migrationStatusService.GetDefaultProfile(8);
        var defaultProfile = _profileConfigService.GetPlan(profileName ?? nameof(BlockMigrationPlan));

        if (defaultProfile == null)
            throw new KeyNotFoundException(nameof(defaultProfile));

        return new MigrationStatus
        {
            Source = defaultProfile.Options.Source,
            Target = defaultProfile.Options.Target,
            Version = 8,
            SiteFolder = "/",
            Icon = "icon-nodes",
            Name = "Site Conversion profile",
            Plan = profileName
        };
    }

    [HttpPost]
    public MigrationResults Validate(MigrationStatus status)
    {
        var defaultProfile = _profileConfigService
            .GetPlan(status.Plan ?? nameof(UpgradeUmbracoSevenPlan));

        if (defaultProfile == null) throw new InvalidOperationException(nameof(defaultProfile));

        var options = _migrationStatusService.ConvertToOptions(status, defaultProfile.Options);
        return _migrationService.Validate(options);
    }


    [HttpGet]
    public IEnumerable<MigrationStatus> GetMigrations()
       => _migrationFileService.GetMigrations();


    [HttpPost]
    public MigrationStatus SaveStatus(MigrationStatus status)
    {
        if (status == null) throw new ArgumentNullException(nameof(status));

        if (status.Id == null)
        {
            _migrationStatusService.CreateNew(status);
            _migrationStatusService.SaveStatus(status.Root, status);
        }
        else
        {
            _ = _migrationStatusService.Get(status.Id)
                ?? throw new InvalidOperationException("can't find status");

            _migrationStatusService.SaveStatus(status.Root, status);
        }

        return status;
    }


    [HttpDelete]
    public void DeleteMigration(string id)
    {
        _migrationFileService.DeleteMigration(id);
    }
}
