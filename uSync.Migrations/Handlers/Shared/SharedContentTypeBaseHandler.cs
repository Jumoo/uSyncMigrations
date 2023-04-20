using System.Xml.Linq;

using Microsoft.Extensions.Logging;

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

using uSync.Core;
using uSync.Migrations.Composing;
using uSync.Migrations.Context;
using uSync.Migrations.Extensions;
using uSync.Migrations.Models;
using uSync.Migrations.Notifications;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Shared;
internal abstract class SharedContentTypeBaseHandler<TEntity> : SharedHandlerBase<TEntity>
    where TEntity : ContentTypeBase
{
    private readonly IDataTypeService _dataTypeService;
    private readonly Lazy<SyncMigrationHandlerCollection> _migrationHandlers;

    protected SharedContentTypeBaseHandler(
		IEventAggregator eventAggregator,
		ISyncMigrationFileService migrationFileService,
		ILogger<SharedContentTypeBaseHandler<TEntity>> logger,
		IDataTypeService dataTypeService,
        Lazy<SyncMigrationHandlerCollection> migrationHandlers)
		: base(eventAggregator, migrationFileService, logger)
	{
        _dataTypeService = dataTypeService;
        _migrationHandlers = migrationHandlers;
    }

	protected override void PrepareFile(XElement source, SyncMigrationContext context)
    {
        var (contentTypeAlias, key) = GetAliasAndKey(source);
        context.ContentTypes.AddAliasAndKey(contentTypeAlias, key);

        var compositions = source.Element("Info")?.Element("Compositions")?.Elements("Composition")?.Select(x => x.Value) ?? Enumerable.Empty<string>();
        context.ContentTypes.AddCompositions(contentTypeAlias, compositions);

        var properties = source.Element("GenericProperties")?.Elements("GenericProperty") ?? Enumerable.Empty<XElement>();

        foreach (var property in properties)
        {
            var editorAlias = property.Element("Type").ValueOrDefault(string.Empty);
            var definition = property.Element("Definition").ValueOrDefault(Guid.Empty);
            var alias = property.Element("Alias")?.ValueOrDefault(string.Empty) ?? string.Empty;
            var propertyName = property.Element("Name")?.ValueOrDefault(string.Empty) ?? string.Empty;

            // if this is a property splitting editor, we need to add the split properties
            var propertySplittingMigrator = context.Migrators.TryGetPropertySplittingMigrator(editorAlias);
            if (propertySplittingMigrator != null)
            {
                context.ContentTypes.AddProperty(contentTypeAlias, alias, editorAlias, "none"); // add the original property so we can reference the original EditorAlias later

                var splitProperties = propertySplittingMigrator.GetSplitProperties(contentTypeAlias, alias, propertyName, context);
                foreach (var splitProperty in splitProperties)
                {
                    context.ContentTypes.AddProperty(contentTypeAlias, splitProperty.Alias,
                                               editorAlias, splitProperty.DataTypeAlias, splitProperty.DataTypeDefinition);

                    context.ContentTypes.AddDataTypeAlias(contentTypeAlias, alias, splitProperty.DataTypeAlias);
                }
            }
            else
            {
                context.ContentTypes.AddProperty(contentTypeAlias, alias,
                    editorAlias, context.DataTypes.GetByDefinition(definition)?.EditorAlias);

                context.ContentTypes.AddDataTypeAlias(contentTypeAlias, alias,
                    context.DataTypes.GetAlias(definition));
            }


            //
            // for now we are doing this just for media folders, but it might be
            // that all list view properties should be ignored ??
            if (contentTypeAlias.Equals("Folder") && editorAlias.Equals("Umbraco.ListView"))
            {
                context.ContentTypes.AddIgnoredProperty(contentTypeAlias, alias);
            }
        }
    }

    protected override XElement? MigrateFile(XElement source, int level, SyncMigrationContext context)
    {
        var info = source.Element("Info");
        if (info == null) return null;

        var (alias, key) = GetAliasAndKey(source);

        var target = new XElement(ItemType,
            new XAttribute(uSyncConstants.Xml.Key, key),
            new XAttribute(uSyncConstants.Xml.Alias, alias),
            new XAttribute(uSyncConstants.Xml.Level, source.GetLevel()));

        // update info element. 
        UpdateInfoSection(info, target, key, context);

        if (ItemType == nameof(ContentType))
        {
            // structure
            UpdateStructure(source, target);
        }

        // properties. 
        UpdateProperties(source, target, alias, context);


        if (ItemType != nameof(ContentType))
        {
            // odd usync thing, in media/member structure is after properties. 
            UpdateStructure(source, target);
        }


        // tabs
        UpdateTabs(source, target, context);

        if (ItemType == nameof(ContentType))
        {
            CheckVariations(target);
        }

        return target;

    }

    protected abstract void UpdateInfoSection(XElement? info, XElement target, Guid key, SyncMigrationContext context);
    protected abstract void UpdateStructure(XElement source, XElement target);
    protected abstract void UpdateTabs(XElement source, XElement target, SyncMigrationContext context);
    protected abstract void CheckVariations(XElement target);

    protected virtual void UpdateProperties(XElement source, XElement target, string alias, SyncMigrationContext context)
    {
        var properties = source.Element("GenericProperties");

        var newProperties = new XElement("GenericProperties");
        if (properties != null)
        {
            foreach (var property in properties.Elements("GenericProperty"))
            {
                var name = property.Element("Name").ValueOrDefault(string.Empty);

                if (context.ContentTypes.IsIgnoredProperty(alias, name))
                {
                    continue;
                }

                var updatedProperties = GetUpdatedProperties(source, alias, property, context);
                newProperties.Add(updatedProperties);
            }
        }

        target.Add(newProperties);
    }

    protected virtual IEnumerable<XElement> GetUpdatedProperties(XElement source, string contentTypeAlias, XElement property, SyncMigrationContext context)
    {
        var editorAlias = property.Element("Type").ValueOrDefault(string.Empty);

        var propertySplittingMigrator = context.Migrators.TryGetPropertySplittingMigrator(editorAlias);
        if (propertySplittingMigrator == null)
        {
            var updatedProperty = GetUpdatedProperty(source, contentTypeAlias, property, context);
            if (updatedProperty != null)
            {
                yield return updatedProperty;
            }

            yield break;
        }
        
        // if we have a property splitting migrator, we need to get the split properties
        var propertyAlias = property.Element("Alias")?.ValueOrDefault(string.Empty) ?? string.Empty;
        var propertyName = property.Element("Name")?.ValueOrDefault(string.Empty) ?? string.Empty;
        var splitProperties = propertySplittingMigrator.GetSplitProperties(contentTypeAlias, propertyAlias, propertyName, context);
        
        foreach (var splitProperty in splitProperties)
        {
            var updatedProperty = GetUpdatedProperty(source, contentTypeAlias, property, context);
            if (updatedProperty != null)
            {
                updatedProperty.CreateOrSetElement("Alias", splitProperty.Alias);
                updatedProperty.CreateOrSetElement("Name", splitProperty.Name);
                updatedProperty.CreateOrSetElement("Type", splitProperty.DataTypeAlias);
                updatedProperty.CreateOrSetElement("Definition", splitProperty.DataTypeDefinition);
                yield return updatedProperty;
            }
        }
    }

    protected virtual XElement? GetUpdatedProperty(XElement source, string alias, XElement property, SyncMigrationContext context)
    {
        var newProperty = property.Clone();
        if (newProperty != null)
        {
            // update the datatype we are using (this might be new). 
            UpdatePropertyEditor(alias, newProperty, context);
            UpdatePropertyXml(source, newProperty, context);
            return newProperty;
        }

        return null;
    }

    protected virtual void UpdatePropertyEditor(string alias, XElement newProperty, SyncMigrationContext context)
    {
        var propertyAlias = newProperty.Element("Alias").ValueOrDefault(string.Empty);

        var updatedType = context.ContentTypes.GetEditorAliasByTypeAndProperty(alias, propertyAlias)?.UpdatedEditorAlias ?? propertyAlias;
        newProperty.CreateOrSetElement("Type", updatedType);

        var definitionElement = newProperty.Element("Definition");
        if (definitionElement == null) return;

        var definition = definitionElement.ValueOrDefault(Guid.Empty);
        var variationValue = "Nothing";

        if (definition != Guid.Empty)
        {
            definitionElement.Value = context.DataTypes.GetReplacement(definition).ToString();
            variationValue = context.DataTypes.GetVariation(definition);
        }

        if (ItemType == nameof(ContentType))
        {
            newProperty.CreateOrSetElement("Variations", variationValue);
        }

    }
    protected abstract void UpdatePropertyXml(XElement source, XElement newProperty, SyncMigrationContext context);


    /// <summary>
    ///  hook into the DoMigration loop so we can add additional doctypes
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
	protected override IEnumerable<MigrationMessage> PreDoMigration(SyncMigrationContext context)
	{
        var messages = new List<MigrationMessage>();
        messages.AddRange(base.PreDoMigration(context));
        messages.AddRange(CreateAdditional(context));
        return messages;
	}

	/// <summary>
	///  Add additional doctypes that might have been added by datatypes during the 
	///  first part of the migration (i.e BlockGrid conversion)
	/// </summary>
	/// <param name="context"></param>
	/// <returns></returns>
	protected IEnumerable<MigrationMessage> CreateAdditional(SyncMigrationContext context) 
    { 
        // this only works on content types (for now)
        if (typeof(TEntity) != typeof(ContentType)) return Enumerable.Empty<MigrationMessage>();

        var messages = new List<MigrationMessage>();

        foreach(var contentType in context.ContentTypes.GetNewContentTypes())
        {
            // if this has been blocked don't add it. 
			if (context.IsBlocked(this.ItemType, contentType.Alias)) continue;

            var source = contentType.MakeXMLFromNewDocType(_dataTypeService);

			var migratingNotification = new SyncMigratingNotification<TEntity>(source, context);
			if (_eventAggregator.PublishCancelable(migratingNotification) == true)
			{
				continue;
			}

			context.ContentTypes.AddAliasAndKey(contentType.Alias, contentType.Key);

            // register any additional properties that might be needed in the following migration
			AddAdditionaProperties(contentType, context);

            // ensure we use the v8 migrator, since the source XML generated above uses the v8 format
            var eightMigrationHandler = this.SourceVersion == 8 ? 
                this : 
                _migrationHandlers.Value.FirstOrDefault(h => h.SourceVersion == 8 && h.ItemType == this.ItemType) as SharedContentTypeBaseHandler<TEntity> ?? this;
            var target = eightMigrationHandler.MigrateFile(source, 1, context);

			if (target != null)
			{
				var migratedNotification =
					new SyncMigratedNotification<TEntity>(target, context).WithStateFrom(migratingNotification);
				_eventAggregator.Publish(migratedNotification);
				messages.Add(SaveTargetXml(context.Metadata.MigrationId, target));
			}
		}
        return messages;
    }

    private void AddAdditionaProperties(NewContentTypeInfo contentType, SyncMigrationContext context)
    {
		foreach (var property in contentType.Properties)
		{
            var dataType = _dataTypeService.GetDataType(property.DataTypeAlias);

            if (dataType != null)
            {
                context.ContentTypes.AddProperty(contentType.Alias, property.Alias,
                    property.OriginalEditorAlias ?? dataType.EditorAlias, dataType.EditorAlias);
            }
		}
	}

}
