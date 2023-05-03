using Archetype.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;
using uSync.Migrations.Context;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Models;

namespace uSync.Migrations.Migrators.Community.Archetype;

[SyncMigrator("Imulus.Archetype")]
[SyncMigratorVersion(7)]
public class ArchetypeToBlockListMigrator : SyncPropertyMigratorBase
{
    public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => UmbConstants.PropertyEditors.Aliases.BlockList;

    public override string GetDatabaseType(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        => nameof(ValueStorageType.Ntext);

    public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
    {
        var config = new BlockListConfiguration();
        var configPrevalue = dataTypeProperty.PreValues?.FirstOrDefault(p => p.Alias == "archetypeConfig")?.Value;
        if (string.IsNullOrEmpty(configPrevalue))
        {
            return string.Empty;
        }

        var archetypeConfiguration = JsonConvert.DeserializeObject<ArchetypePreValue>(configPrevalue);

        if (archetypeConfiguration == null)
        {
            return config;
        }

        config.ValidationLimit = new BlockListConfiguration.NumberRange()
        {
            Max = archetypeConfiguration.MaxFieldsets == default ? null : archetypeConfiguration.MaxFieldsets,
            Min = archetypeConfiguration.MinFieldsets == default ? null : archetypeConfiguration.MinFieldsets,
        };
        config.UseSingleBlockMode = archetypeConfiguration.MaxFieldsets == 1 && archetypeConfiguration.MinFieldsets == 1 && archetypeConfiguration.Fieldsets.Count() == 1;
        config.UseLiveEditing = true;
        config.UseInlineEditingAsDefault = true;

        var blocks = new List<BlockListConfiguration.BlockConfiguration>();

        foreach (var fieldset in archetypeConfiguration.Fieldsets)
        {
            var alias = context.ContentTypes.ArchetypeMigrationConfigurer.GetBlockElementAlias(fieldset.Alias, context);
            var newContentType = new NewContentTypeInfo
            {
                Key = alias.ToGuid(),
                Alias = alias,
                Icon = string.IsNullOrWhiteSpace(fieldset.Icon) ? "icon-umb-content" : fieldset.Icon,
                IsElement = true,
                Name = fieldset.Label,
                Folder = "Block List",
                Properties = fieldset.Properties
                    .Select(p =>
                    {
                        var dataType = context.DataTypes.GetByDefinition(p.DataTypeGuid);
                        if (dataType == null)
                        {
                            return null;
                        }

                        return new NewContentTypeProperty
                        {
                            Alias = p.Alias,
                            Name = p.Label,
                            DataTypeAlias = dataType.DataTypeName,
                            OriginalEditorAlias = dataType.EditorAlias,
                        };
                    })
                    .WhereNotNull()
                    .ToList(),
            };

            context.ContentTypes.AddNewContentType(newContentType);
            context.ContentTypes.AddAliasAndKey(newContentType.Alias, newContentType.Key);
            context.ContentTypes.AddElementType(newContentType.Key);

            blocks.Add(new BlockListConfiguration.BlockConfiguration
            {
                ContentElementTypeKey = newContentType.Key,
                Label = fieldset.LabelTemplate,
            });
        }

        config.Blocks = blocks.ToArray();

        return config;
    }

    public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
    {
        if (string.IsNullOrWhiteSpace(contentProperty?.Value))
        {
            return string.Empty;
        }

        var archetype = JsonConvert.DeserializeObject<ArchetypeModel>(contentProperty.Value);
        if (archetype == null)
        {
            return string.Empty;
        }

        var items = archetype.Fieldsets?.Where(f => !f.Disabled);

        if (items?.Any() != true)
        {
            return string.Empty;
        }

        var contentData = new List<BlockItemData>();

        var layout = new List<BlockListLayoutItem>();

        foreach (var item in items)
        {
            var blockElementAlias = context.ContentTypes.ArchetypeMigrationConfigurer.GetBlockElementAlias(item.Alias, context);
            var rawValues = new Dictionary<string, object?>();
            foreach (var property in item.Properties)
            {
                var editorAlias = context.ContentTypes.GetEditorAliasByTypeAndProperty(blockElementAlias, property.Alias);

                if (editorAlias == null)
                {
                    continue;
                }

                var migrator = context.Migrators.TryGetMigrator(editorAlias.OriginalEditorAlias);

                if (migrator == null)
                {
                    continue;
                }

                var childProperty = new SyncMigrationContentProperty(editorAlias.OriginalEditorAlias,
                    property.Value.ToString() ?? string.Empty);

                rawValues[property.Alias] = migrator.GetContentValue(childProperty, context);
            }

            var key = context.ContentTypes.GetKeyByAlias(blockElementAlias);
            var block = new BlockItemData
            {
                ContentTypeKey = key,
                Udi = Udi.Create(UmbConstants.UdiEntityType.Element, item.Id),
                RawPropertyValues = rawValues,
            };

            layout.Add(new BlockListLayoutItem { ContentUdi = block.Udi });

            contentData.Add(block);
        }

        if (contentData.Any() == false)
        {
            return string.Empty;
        }

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
