using System.Globalization;
using System.Xml.Linq;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

using uSync.Core;
using uSync.Migrations.Extensions;
using uSync.Migrations.Models;
using uSync.Migrations.Notifications;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers;

[SyncMigrtionHandler(BackOfficeConstants.Groups.Settings, uSyncMigrations.Priorities.Languages, 7,
    SourceFolderName = "Languages", TargetFolderName = "Languages")]
internal class LanguageMigrationHandler : MigrationHandlerBase<Language>, ISyncMigrationHandler
{
    private readonly ILocalizationService _localizationService;

    public LanguageMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        ILocalizationService localizationService)
        : base(eventAggregator, migrationFileService)
    {
        _localizationService = localizationService;
    }

    protected override void PrepareFile(XElement source, SyncMigrationContext context)
    { }

    protected override XElement? MigrateFile(XElement source, int level, SyncMigrationContext context)
    {
        var alias = source.Attribute("CultureAlias").ValueOrDefault(string.Empty);

        var existing = _localizationService.GetLanguageByIsoCode(alias);

        var culture = CultureInfo.GetCultureInfo(alias);
        var key = culture.LCID.Int2Guid();

        var target = new XElement("Language",
            new XAttribute(uSyncConstants.Xml.Key, key),
            new XAttribute(uSyncConstants.Xml.Alias, alias),
            new XAttribute(uSyncConstants.Xml.Level, 0),

            new XElement("IsoCode", alias),
            new XElement("IsMandatory", existing?.IsMandatory ?? false),
            new XElement("IsDefault", existing?.IsDefault ?? false));

        return target;
    }

}
