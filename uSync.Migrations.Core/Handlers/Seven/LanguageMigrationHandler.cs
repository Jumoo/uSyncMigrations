using System.Xml.Linq;

using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

using uSync.Core;
using uSync.Migrations.Core.Context;
using uSync.Migrations.Core.Handlers.Shared;
using uSync.Migrations.Core.Services;

namespace uSync.Migrations.Core.Handlers.Seven;

[SyncMigrationHandler(BackOfficeConstants.Groups.Settings, uSyncMigrations.Priorities.Languages,
    SourceVersion = 7,
    SourceFolderName = "Languages", TargetFolderName = "Languages")]
public class LanguageMigrationHandler : SharedHandlerBase<Language>, ISyncMigrationHandler
{
    private readonly ILocalizationService _localizationService;

    public LanguageMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        ILocalizationService localizationService,
        ILogger<LanguageMigrationHandler> logger)
        : base(eventAggregator, migrationFileService, logger)
    {
        _localizationService = localizationService;
    }

    protected override (string alias, Guid key) GetAliasAndKey(XElement source, SyncMigrationContext? context)
    {
        var alias = source.Attribute("CultureAlias").ValueOrDefault(string.Empty);
        var key = alias.ToGuid();
        return (alias, key);
    }
    protected override XElement? MigrateFile(XElement source, int level, SyncMigrationContext context)
    {
        var (alias, key) = GetAliasAndKey(source, context);

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
