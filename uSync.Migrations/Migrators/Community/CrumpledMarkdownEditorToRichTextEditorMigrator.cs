using HeyRed.MarkdownSharp;
using Newtonsoft.Json;

using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Core;

using uSync.Migrations.Context;
using uSync.Migrations.Migrators.Models;
using uSync.Migrations.Helpers;

namespace uSync.Migrations.Migrators.Community
{
    [SyncMigrator("Crumpled.MarkdownEditor")]
    public class CrumpledMarkdownEditorToRichTextEditorMigrator : SyncPropertyMigratorBase
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public CrumpledMarkdownEditorToRichTextEditorMigrator(
            IUmbracoContextFactory umbracoContextFactory)
        {
            _umbracoContextFactory = umbracoContextFactory;
        }

        public override string GetEditorAlias(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
      => UmbConstants.PropertyEditors.Aliases.TinyMce;
        public override object? GetConfigValues(SyncMigrationDataTypeProperty dataTypeProperty, SyncMigrationContext context)
        {
            var config = new RichTextConfiguration();
            return config;
        }

        public override string? GetContentValue(SyncMigrationContentProperty contentProperty, SyncMigrationContext context)
        {
            var markdownContent = JsonConvert.DeserializeObject<CrumpledMarkDown>(contentProperty.Value)?.Editor.Content;

            //If the markdown can't be deserialised then return an empty string.
            if (markdownContent == null) return string.Empty;

            var markdown = new Markdown();
            markdownContent = markdown.Transform(markdownContent);

            using (UmbracoContextReference umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext())
            {
                markdownContent = MarkdownEditorHelper.ParseUmbracoMarkDownLinks(markdownContent, umbracoContextReference);
            }

            if (!string.IsNullOrWhiteSpace(markdownContent))
            {
                //Return and remove any random whitespaces from the end.
                return markdownContent.Trim();
            }
            return markdownContent;
        }
    }
}
