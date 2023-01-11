using System.Xml.Linq;

using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Umbraco.Cms.Core.ContentApps;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using uSync.Core;
using uSync.Migrations.Models;
using uSync.Migrations.Notifications;
using uSync.Migrations.Services;

namespace uSync.Migrations.Handlers.Shared;
internal abstract class SharedContentTypeBaseHandler<TEntity> : SharedHandlerBase<TEntity>
    where TEntity : ContentTypeBase
{
    protected SharedContentTypeBaseHandler(
        IEventAggregator eventAggregator,
        ISyncMigrationFileService migrationFileService,
        ILogger<SharedContentTypeBaseHandler<TEntity>> logger) 
        : base(eventAggregator, migrationFileService, logger)
    { }

    protected override void PrepareFile(XElement source, SyncMigrationContext context)
    {
        var (contentTypeAlias, key) = GetAliasAndKey(source);
        context.AddContentTypeKey(contentTypeAlias, key);

        var compositions = source.Element("Info")?.Element("Compositions")?.Elements("Composition")?.Select(x => x.Value) ?? Enumerable.Empty<string>();
        context.AddContentTypeCompositions(contentTypeAlias, compositions);

        var properties = source.Element("GenericProperties")?.Elements("GenericProperty") ?? Enumerable.Empty<XElement>();

        foreach (var property in properties)
        {
            var editorAlias = property.Element("Type").ValueOrDefault(string.Empty);
            var definition = property.Element("Definition").ValueOrDefault(Guid.Empty);
            var alias = property.Element("Alias")?.ValueOrDefault(string.Empty) ?? string.Empty;

            context.AddContentProperty(contentTypeAlias, alias,
                    editorAlias, context.GetDataTypeFromDefinition(definition));

            //
            // for now we are doing this just for media folders, but it might be
            // that all list view properties should be ignored ??
            if (contentTypeAlias.Equals("Folder") && editorAlias.Equals("Umbraco.ListView"))
            {
                context.AddIgnoredProperty(contentTypeAlias, alias);
            }
        }
    }

    public override IEnumerable<MigrationMessage> DoMigration(SyncMigrationContext context)
    {
        var messages = base.DoMigration(context).ToList();

        if (typeof(TEntity) == typeof(ContentType))
        {
            foreach (var (contentTypeKey, (contentTypeAlias, name)) in context.GetAdditionalContentTypes())
            {
                try
                {
                    var source = XElement.Parse(@"<ContentType>
                    <Info>
                        <Name></Name>
                        <Icon>icon-book-alt color-black</Icon>
                        <Thumbnail></Thumbnail>
                        <Description></Description>
                        <AllowAtRoot>False</AllowAtRoot>
                        <IsListView>False</IsListView>
                        <Variations>Nothing</Variations>
                        <IsElement>true</IsElement>
                        <HistoryCleanup>
                            <PreventCleanup>False</PreventCleanup>
                            <KeepAllVersionsNewerThanDays></KeepAllVersionsNewerThanDays>
                            <KeepLatestVersionPerDayForDays></KeepLatestVersionPerDayForDays>
                        </HistoryCleanup>
                        <Compositions />
                        <DefaultTemplate></DefaultTemplate>
                        <AllowedTemplates />
                    </Info>
                    <Structure />
                    <GenericProperties />
                    <Tabs />
                    </ContentType>");

                    source.SetAttributeValue(uSyncConstants.Xml.Key, contentTypeKey);
                    source.SetAttributeValue(uSyncConstants.Xml.Alias, contentTypeAlias);
                    source.SetAttributeValue(uSyncConstants.Xml.Level, 1);

                    var info = source.FindOrCreate(uSyncConstants.Xml.Info);
                    info.SetElementValue(uSyncConstants.Xml.Name, name ?? contentTypeAlias);
                    info.SetElementValue(uSyncConstants.Xml.Key, contentTypeKey);
                    info.SetElementValue(uSyncConstants.Xml.Alias, contentTypeAlias);

                    var (alias, key) = GetAliasAndKey(source);
                    if (context.IsBlocked(ItemType, alias)) continue;

                    var migratingNotification = new SyncMigratingNotification<TEntity>(source, context);
                    if (_eventAggregator.PublishCancelable(migratingNotification) == true)
                    {
                        continue;
                    }

                    var target = MigrateFile(source, 1, context);

                    if (target != null)
                    {
                        var migratedNotification =
                            new SyncMigratedNotification<TEntity>(target, context).WithStateFrom(migratingNotification);
                        _eventAggregator.Publish(migratedNotification);
                        messages.Add(SaveTargetXml(context.MigrationId, target));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while processing additional content Type {alias}", contentTypeAlias);
                    throw new Exception($"Error processing additional content Type {contentTypeAlias}", ex);
                }
            }
        }

        return messages;
    }

    protected override XElement? MigrateFile(XElement source, int level, SyncMigrationContext context)
    {
        var info = source.Element(uSyncConstants.Xml.Info);
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
        UpdateTabs(source, target);

        if (ItemType == nameof(ContentType))
        {
            CheckVariations(target);
        }

        return target;
    }

    protected abstract void UpdateInfoSection(XElement? info, XElement target, Guid key, SyncMigrationContext context);
    protected abstract void UpdateStructure(XElement source, XElement target);
    protected abstract void UpdateTabs(XElement source, XElement target);
    protected abstract void CheckVariations(XElement target);

    protected virtual void UpdateProperties(XElement source, XElement target, string alias, SyncMigrationContext context)
    {
        var properties = source.Element("GenericProperties");

        var newProperties = new XElement( "GenericProperties");
        if (properties != null)
        {
            foreach (var property in properties.Elements("GenericProperty"))
            {
                var name = property.Element("Name").ValueOrDefault(string.Empty);

                if (context.IsIgnoredProperty(alias, name))
                {
                    continue;
                }

                var newProperty = XElement.Parse(property.ToString());

                // update the datatype we are using (this might be new). 
                UpdatePropertyEditor(alias, newProperty, context);

                UpdatePropertyXml(newProperty);

                newProperties.Add(newProperty);
            }
        }

        target.Add(newProperties);
    }

    protected virtual void UpdatePropertyEditor(string alias, XElement newProperty, SyncMigrationContext context)
    {
        var propertyAlias = newProperty.Element("Alias").ValueOrDefault(string.Empty);

        var updatedType = context.GetEditorAlias(alias, propertyAlias)?.UpdatedEditorAlias ?? propertyAlias;
        newProperty.CreateOrSetElement("Type", updatedType);

        var definitionElement = newProperty.Element("Definition");
        if (definitionElement == null) return;

        var definition = definitionElement.ValueOrDefault(Guid.Empty);
        var variationValue = "Nothing";

        if (definition != Guid.Empty)
        {
            definitionElement.Value = context.GetReplacementDataType(definition).ToString();
            variationValue = context.GetDataTypeVariation(definition);
        }

        if (ItemType == nameof(ContentType))
        {
            newProperty.CreateOrSetElement("Variations", variationValue);
        }

    }
    protected abstract void UpdatePropertyXml(XElement newProperty);
}
