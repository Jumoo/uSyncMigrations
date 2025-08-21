using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;
using uSync.Core;
using uSync.Migrations.Core.Context;
using uSync.Migrations.Core.Handlers.Shared;
using uSync.Migrations.Core.Services;

namespace uSync.Migrations.Core.Handlers.Seven;

[SyncMigrationHandler(BackOfficeConstants.Groups.Settings, uSyncMigrations.Priorities.Domains,
    SourceVersion = 7,
    SourceFolderName = "Domains", TargetFolderName = "Domains")]
internal class DomainMigrationHandler : SharedHandlerBase<IDomain>, ISyncMigrationHandler
{
    public DomainMigrationHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        ILogger<DomainMigrationHandler> logger)
        : base(eventAggregator, migrationFileService, logger)
    {
    }

    protected override (string alias, Guid key) GetAliasAndKey(XElement source, SyncMigrationContext? context)
    {
        // For v7 domains, we need to extract from the old format
        var domainName = source.Element("DomainName")?.Value ?? string.Empty;
        
        // Generate a deterministic GUID from the domain name
        var key = GenerateDeterministicGuid(domainName);
        
        return (domainName, key);
    }

    protected override XElement? MigrateFile(XElement source, int level, SyncMigrationContext context)
    {
        // Extract Umbraco 7 domain properties
        var domainName = source.Element("DomainName")?.Value;
        if (string.IsNullOrEmpty(domainName))
        {
            _logger.LogWarning("Domain file has no DomainName element, skipping");
            return null;
        }

        var isWildcard = source.Element("IsWildcard")?.Value?.ToLower() == "true";
        var languageId = source.Element("LanguageId")?.Value;
        var rootContentElement = source.Element("RootContent");
        var rootContentKey = rootContentElement?.Attribute("Key")?.Value;
        var rootContentName = rootContentElement?.Value;

        // Convert language ID to culture code
        var culture = ConvertLanguageIdToCulture(languageId, context);
        
        // Generate deterministic GUID for the domain
        var domainKey = GenerateDeterministicGuid(domainName);
        
        // Determine content path 
        var contentPath = DetermineContentPath(domainName, rootContentName, rootContentKey, context);

        // Create Umbraco 13 domain structure
        var target = new XElement("Domain",
            new XAttribute(uSyncConstants.Xml.Key, domainKey),
            new XAttribute(uSyncConstants.Xml.Alias, domainName),
            new XElement("Info",
                new XElement("IsWildcard", isWildcard),
                new XElement("Language", culture),
                new XElement("Root", 
                    new XAttribute(uSyncConstants.Xml.Key, rootContentKey ?? Guid.Empty.ToString()),
                    contentPath),
                new XElement("SortOrder", level)
            )
        );

        _logger.LogInformation("Migrated domain: {domain} -> {path}", domainName, contentPath);
        return target;
    }

    /// <summary>
    /// Convert Umbraco 7 language ID to culture code
    /// </summary>
    private string ConvertLanguageIdToCulture(string? languageId, SyncMigrationContext context)
    {
        // Default mapping for common language IDs
        return (languageId ?? "1") switch
        {
            "1" => "en-US",  // English (United States)
            "2" => "en-GB",  // English (United Kingdom)  
            "3" => "en-AU",  // English (Australia)
            "4" => "fr-FR",  // French
            "5" => "de-DE",  // German
            "6" => "es-ES",  // Spanish
            _ => "en-US"     // Default to US English
        };
    }

    /// <summary>
    /// Generate a deterministic GUID from the domain name
    /// </summary>
    private Guid GenerateDeterministicGuid(string input)
    {
        if (string.IsNullOrEmpty(input))
            return Guid.Empty;

        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input.ToLower()));
        return new Guid(hash);
    }

    /// <summary>
    /// Determine the content path based on domain name and root content
    /// </summary>
    private string DetermineContentPath(string domainName, string? rootContentName, string? rootContentKey, SyncMigrationContext context)
    {
        // If we have a root content name, use it
        if (!string.IsNullOrEmpty(rootContentName))
        {
            // Check if it's just "Home" - then try to determine from domain
            if (rootContentName.Equals("Home", StringComparison.OrdinalIgnoreCase))
            {
                return DeterminePathFromDomain(domainName);
            }
            
            return $"/{rootContentName}";
        }

        // Try to determine from domain name
        return DeterminePathFromDomain(domainName);
    }

    /// <summary>
    /// Determine content path from domain name patterns
    /// </summary>
    private string DeterminePathFromDomain(string domainName)
    {
        var domain = domainName.ToLower();

        // This is a generic implementation - specific sites can override via context
        // The implementation should be customizable via configuration
        
        // Default to root
        return "/";
    }
}