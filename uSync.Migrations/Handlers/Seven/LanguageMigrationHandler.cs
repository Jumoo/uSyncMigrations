using System.Globalization;
using System.Xml.Linq;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

using uSync.Core;
using uSync.Migrations.Extensions;
using uSync.Migrations.Handlers.Shared;
using uSync.Migrations.Models;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Seven;

[SyncMigrationHandler(BackOfficeConstants.Groups.Settings, uSyncMigrations.Priorities.Languages, 
    SourceVersion = 7,
    SourceFolderName = "Languages", TargetFolderName = "Languages")]
internal class LanguageMigrationHandler : SharedHandlerBase<Language>, ISyncMigrationHandler
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

    protected override (string alias, Guid key) GetAliasAndKey(XElement source)
    {
        var alias = source.Attribute("CultureAlias").ValueOrDefault(string.Empty);
        var key = CultureInfo.GetCultureInfo(alias)?.LCID.Int2Guid() ?? Guid.Empty;
        return (alias: alias, key: key);
    }
    protected override XElement? MigrateFile(XElement source, int level, SyncMigrationContext context)
    {
        var (alias, key) = GetAliasAndKey(source);

        var existing = _localizationService.GetLanguageByIsoCode(alias);

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
