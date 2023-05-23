using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Migrators.Models;

namespace uSync.Migrations.Migrators;

[SyncMigrator("Umbraco.RelatedLinks")]
[SyncMigrator("Umbraco.RelatedLinks2")]
public class RelatedLinksMigrator : SyncPropertyMigratorBase
{
    private readonly JsonSerializerSettings _serializerSettings;

    public RelatedLinksMigrator()
    {
        _serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new List<JsonConverter> { new StringEnumConverter() },
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };
    }

    private string WrapGuidsWithQuotes(string value)
    {       
      string guidRegEx = @"\b[A-Fa-f0-9]{8}(?:-[A-Fa-f0-9]{4}){3}-[A-Fa-f0-9]{12}\b";
        HashSet<string> uniqueMatches = new HashSet<string>();

        foreach (Match m in Regex.Matches(value, guidRegEx)) {
            uniqueMatches.Add(m.Value);
        }

        foreach (var guid in uniqueMatches) { 
          value = value.Replace(guid, "\"" + guid + "\"")
                .Replace("\"\"", "\"");
        }
        return value;
    }

    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.MultiUrlPicker;

    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var mappings = new Dictionary<string, string>
        {
            { "max", nameof(MultiUrlPickerConfiguration.MaxNumber) },
        };

        return new MultiUrlPickerConfiguration().MapPreValues(dataTypeProperty.PreValues, mappings);
    }

    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        var links = new List<Link>();

        if (string.IsNullOrWhiteSpace(contentProperty.Value) == false)
        {
            //uSync Content edition turns RelatedLinks Ids into Guids for syncing between environments, but they are not wrapped in double quotes, and so in this context can't be deserialized
            //so we need to 'fangle' the Value here to wrap any guids in the json in double quotes before it's parsed.
            var wrappedValue = WrapGuidsWithQuotes(contentProperty.Value);
            var items = JsonConvert.DeserializeObject<List<RelatedLink>>(wrappedValue);
            if (items?.Any() == true)
            {
                foreach (var item in items)
                {
                    var udi = item.IsInternal == true && Guid.TryParse(item.Link, out var guid) == true
                        ? Udi.Create(UmbConstants.UdiEntityType.Document, guid)
                        : Udi.Create(UmbConstants.UdiEntityType.Unknown);

                    links.Add(new Link
                    {
                        Name = item.Caption,
                        Target = item.NewWindow == true ? "_blank" : null,
                        Type = item.IsInternal == true ? LinkType.Content : LinkType.External,
                        Udi = item.IsInternal == true && udi.EntityType == UmbConstants.UdiEntityType.Document ? udi : null,
                        Url = item.IsInternal == false ? item.Link : null,
                    });
                }
            }
        }

        return JsonConvert.SerializeObject(links, _serializerSettings);
    }

    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    private class RelatedLink
    {
        public int? Id { get; internal set; }

        public string? Caption { get; set; }

        internal bool IsDeleted { get; set; }

        public string? Link { get; set; }

        public bool NewWindow { get; set; }

        public bool IsInternal { get; set; }
    }
}
