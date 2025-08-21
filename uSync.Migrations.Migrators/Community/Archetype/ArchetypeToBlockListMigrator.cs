using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;
using uSync.Migrations.Migrators.Community.Archetype.Models;

namespace uSync.Migrations.Migrators.Community.Archetype;

[SyncMigrator("Imulus.Archetype")]
[SyncMigratorVersion(7)]
public class ArchetypeToBlockListMigrator : SyncPropertyMigratorBase
{
    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        DateParseHandling = DateParseHandling.None
    };

    private readonly IArchetypeAliasResolver _archetypeAliasResolver;

    public ArchetypeToBlockListMigrator(IArchetypeAliasResolver archetypeAliasResolver)
    {
        _archetypeAliasResolver = archetypeAliasResolver;
    }

    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.BlockList;

    public override string GetDatabaseType(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => nameof(ValueStorageType.Ntext);

    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty,
        SyncMigrationContext context)
    {
        var config = new BlockListConfiguration();

        var configPreValue = dataTypeProperty.PreValues?.FirstOrDefault(p => p.Alias == "archetypeConfig")?.Value;

        if (string.IsNullOrEmpty(configPreValue))
            return string.Empty;

        var archetypeConfiguration = JsonConvert.DeserializeObject<ArchetypePreValue>(configPreValue);

        if (archetypeConfiguration is null)
            return config;

        config.ValidationLimit = new BlockListConfiguration.NumberRange
        {
            Max = archetypeConfiguration.MaxFieldsets == default ? null : archetypeConfiguration.MaxFieldsets,
            Min = archetypeConfiguration.MinFieldsets == default ? null : archetypeConfiguration.MinFieldsets,
        };

        config.UseSingleBlockMode = archetypeConfiguration.MaxFieldsets == 1 &&
                                    archetypeConfiguration.MinFieldsets == 1 &&
                                    archetypeConfiguration.Fieldsets.Count() == 1;

        config.UseLiveEditing = true;

        config.UseInlineEditingAsDefault = true;

        var blocks = new List<BlockListConfiguration.BlockConfiguration>();

        foreach (var fieldSet in archetypeConfiguration.Fieldsets)
        {
            var alias = _archetypeAliasResolver?.GetBlockElementAlias(fieldSet.Alias!, dataTypeProperty.DataTypeAlias);

            if (string.IsNullOrEmpty(alias))
                continue;

            var newContentType = new NewContentTypeInfo(
                key: alias.ToGuid(),
                alias: alias,
                name: fieldSet.Label ?? alias,
                icon: string.IsNullOrWhiteSpace(fieldSet.Icon) ? "icon-umb-content" : fieldSet.Icon,
                folder: "BlockList")
            {
                IsElement = true
            };

            if (fieldSet.Properties != null)
            {
                newContentType.Properties = fieldSet.Properties
                    .Select(p =>
                    {
                        var dataType = context.DataTypes.GetByDefinition(p.DataTypeGuid);

                        if (dataType == null)
                            return null;

                        return new NewContentTypeProperty(
                            alias: p.Alias!,
                            name: p.Label ?? p.Alias!,
                            dataTypeAlias: dataType.DataTypeName,
                            orginalEditorAlias: string.IsNullOrWhiteSpace(p.PropertyEditorAlias)
                                ? dataType.OriginalEditorAlias
                                : p.PropertyEditorAlias);
                    })
                    .WhereNotNull()
                    .ToList();
            }

            context.ContentTypes.AddNewContentType(newContentType);
            context.ContentTypes.AddAliasAndKey(newContentType.Alias, newContentType.Key);
            context.ContentTypes.AddElementType(newContentType.Key);

            blocks.Add(new BlockListConfiguration.BlockConfiguration
            {
                ContentElementTypeKey = newContentType.Key,
                Label = fieldSet.LabelTemplate.IfNullOrWhiteSpace(fieldSet.Label),
            });
        }

        config.Blocks = blocks.ToArray();

        return config;
    }

    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (string.IsNullOrWhiteSpace(contentProperty?.Value))
            return string.Empty;

        var archetype = JsonConvert.DeserializeObject<ArchetypeModel>(contentProperty.Value, SerializerSettings);

        if (archetype is null)
            return string.Empty;

        var items = archetype.Fieldsets
            .Where(f => !f.Disabled)
            .ToList();

        if (!items.Any())
            return string.Empty;

        var contentData = new List<BlockItemData>();

        var layout = new List<BlockListLayoutItem>();

        var dataTypeAlias = _archetypeAliasResolver.GetDataTypeAlias(contentProperty, context);

        foreach (var item in items)
        {
            var blockElementAlias = _archetypeAliasResolver.GetBlockElementAlias(item.Alias!, dataTypeAlias);

            if (string.IsNullOrEmpty(blockElementAlias))
                continue;

            var rawValues = new Dictionary<string, object?>();

            foreach (var property in item.Properties)
            {
                if (string.IsNullOrEmpty(property.Alias))
                    continue;

                var editorAlias = context.ContentTypes
                    .GetEditorAliasByTypeAndProperty(blockElementAlias, property.Alias);

                if (editorAlias is null)
                    continue;

                var migrator = context.Migrators.TryGetMigrator(editorAlias.OriginalEditorAlias);

                if (migrator is null)
                {
                    rawValues[property.Alias] = property.Value;
                    continue;
                }

                var childProperty = new SyncMigrationContentProperty(
                    contentTypeAlias: blockElementAlias,
                    propertyAlias: property.Alias,
                    editorAlias: editorAlias.OriginalEditorAlias,
                    value: property.Value?.ToString() ?? string.Empty);

                rawValues[property.Alias] = migrator.GetContentValue(childProperty, context);
            }

            var key = context.ContentTypes.GetKeyByAlias(blockElementAlias);

            var block = new BlockItemData
            {
                ContentTypeKey = key,
                Udi = Udi.Create(UmbConstants.UdiEntityType.Element, item.Id),
                RawPropertyValues = rawValues,
            };

            layout.Add(new BlockListLayoutItem
            {
                ContentUdi = block.Udi
            });

            contentData.Add(block);
        }

        if (!contentData.Any())
            return string.Empty;

        var model = new BlockValue
        {
            ContentData = contentData,
            Layout = new Dictionary<string, JToken>
            {
                { UmbConstants.PropertyEditors.Aliases.BlockList, JArray.FromObject(layout) },
            },
        };

        return JsonConvert.SerializeObject(model, Formatting.Indented);
    }
}