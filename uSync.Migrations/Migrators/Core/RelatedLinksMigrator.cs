using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using uSync.Migrations.Extensions;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators;

internal class RelatedLinksMigrator : SyncPropertyMigratorBase
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

    public override string[] Editors => new[]
    {
        "Umbraco.RelatedLinks",
        "Umbraco.RelatedLinks2"
    };

    public override string GetEditorAlias(string editorAlias, string databaseType, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.MultiUrlPicker;

    public override object GetConfigValues(string editorAlias, string databaseType, IList<PreValue> preValues, SyncMigrationContext context)
    {
        var mappings = new Dictionary<string, string>
        {
            { "max", nameof(MultiUrlPickerConfiguration.MaxNumber) },
        };

        return new MultiUrlPickerConfiguration().MapPreValues(preValues, mappings);
    }

    public override string GetContentValue(string editorAlias, string value, SyncMigrationContext context)
    {
        var links = new List<Link>();

        if (string.IsNullOrWhiteSpace(value) == false)
        {
            var items = JsonConvert.DeserializeObject<List<RelatedLink>>(value);
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
