using System.Web;

using HtmlAgilityPack;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;

namespace uSync.Migrations.Helpers
{
    public static class MarkdownEditorHelper
    {
        public static string ParseUmbracoMarkDownLinks(this string html, UmbracoContextReference umbracoContextReference)
        {
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Find all images with rel attribute
                var linkNodes = doc.DocumentNode.SelectNodes("//a[@href]");

                if (linkNodes != null)
                {
                    var modified = false;

                    foreach (var link in linkNodes)
                    {
                        var firstOrDefault = link.Attributes.FirstOrDefault(x => x.Name.Equals("href"));
                        if (firstOrDefault != null && firstOrDefault.Value.StartsWith("/umbLink:true"))
                        {
                            var value = firstOrDefault.Value.Substring(13);

                            if (value.Contains("/extLink:"))
                            {
                                var posExt = value.IndexOf("/extLink:");
                                var extLink = value.Substring(posExt + 9);
                                value = value.Substring(0, posExt);
                                firstOrDefault.Value = extLink;
                            }

                            if (value.Length > 0)
                            {

                                var linkVariables =
                                    value.Substring(1)
                                        .Split('/')
                                        .Select(variable => variable.Split(':'))
                                        .ToDictionary(pair => pair[0], pair => pair[1]);

                                if (linkVariables.ContainsKey("target"))
                                {
                                    var parseTarget = linkVariables.FirstOrDefault(x => x.Key == "target");
                                    link.Attributes.Add("target", parseTarget.Value);
                                }

                                if (linkVariables.ContainsKey("title"))
                                {
                                    var parseTitle = linkVariables.FirstOrDefault(x => x.Key == "title");

                                    var title = HttpUtility.UrlDecode(parseTitle.Value);

                                    link.Attributes.Add("title", title);
                                }

                                if (linkVariables.ContainsKey("localLink"))
                                {
                                    int nodeId;
                                    int.TryParse(
                                        linkVariables.FirstOrDefault(x => x.Key == "localLink").Value,
                                        out nodeId);

                                    // this should always be changed except for NUnit tests
                                    var newLinkUrl = "/testing/";
                                    if (umbracoContextReference != null)
                                    {
                                        IPublishedContentCache? contentCache = umbracoContextReference.UmbracoContext.Content;
                                        IPublishedContent? siteRoot = contentCache?.GetAtRoot().FirstOrDefault();
                                        if (siteRoot != null && siteRoot.Children?.Any() == true)
                                        {
                                            newLinkUrl = (siteRoot?.Children.Where(x => x.Id == nodeId).FirstOrDefault())?.Url() ?? string.Empty;
                                        }
                                        else
                                        {
                                            newLinkUrl = "Could not find node for link - Node Id:" + nodeId;
                                        }
                                    }

                                    if (newLinkUrl == "#")
                                    {
                                        IPublishedMediaCache? mediaCache = umbracoContextReference?.UmbracoContext.Media;
                                        IPublishedContent? siteRoot = mediaCache?.GetAtRoot().FirstOrDefault();

                                        var mediaItem = siteRoot?.FirstChild(f => f.Id == nodeId) ?? null;
                                        if (mediaItem != null)
                                        {
                                            newLinkUrl = mediaItem.Url();
                                        }
                                        else
                                        {
                                            newLinkUrl = null;
                                        }
                                    }
                                    if (newLinkUrl != null)
                                    {
                                        firstOrDefault.Value = newLinkUrl;
                                    }
                                    else
                                    {
                                        // Link is invalid lets remove it and log
                                        link.ParentNode.RemoveChild(link, true);
                                    }
                                }
                            }

                            modified = true;
                        }
                    }

                    if (modified)
                    {
                        return doc.DocumentNode.OuterHtml;
                    }
                }

                return html;
            }
            catch 
            {
                // log an error ? 
                return html;
            }
        }
    }
}
