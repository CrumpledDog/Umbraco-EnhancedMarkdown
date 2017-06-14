using System;
using System.Linq;
using System.Web;
using HtmlAgilityPack;
using MarkdownSharp;
using Our.Umbraco.EnhancedMarkdown.Models;
using Umbraco.Core.Logging;
using Umbraco.Web;

namespace Our.Umbraco.EnhancedMarkdown
{
    public static class MarkdownHelpers
    {
        public static IHtmlString ExtractHtml(string json)
        {
            var converted = string.Empty;

            if (!string.IsNullOrEmpty(json) && ConversionHelpers.IsJson(json))
            {
                var convertedModel = json.TryConvertToModel<MarkdownEditor>();
                var transformed = ParseMarkDown(convertedModel.Editor.Content);
                converted = transformed.ParseUmbracoMarkDownLinks();
            }

            return new HtmlString(converted);
        }

        public static string ParseMarkDown(string markdownStr)
        {
            var markdown = new Markdown();
            return markdown.Transform(markdownStr);
        }

        public static string ParseUmbracoMarkDownLinks(this string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            UmbracoHelper umbracoHelper = null;

            if (UmbracoContext.Current != null)
            {
                umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            }

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
                                    if (umbracoHelper != null)
                                    {
                                        newLinkUrl = umbracoHelper.NiceUrl(nodeId);
                                    }

                                    if (newLinkUrl == "#")
                                    {
                                        var mediaItem = umbracoHelper.TypedMedia(nodeId);
                                        newLinkUrl = mediaItem?.Url;
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
                                if (linkVariables.ContainsKey("anchor"))
                                {

                                    if (firstOrDefault.Value.StartsWith("/umbLink:true"))
                                    {
                                        firstOrDefault.Value = "#" + linkVariables["anchor"];
                                    }
                                    else
                                    {
                                        firstOrDefault.Value = firstOrDefault.Value + "#" + linkVariables["anchor"];
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
            catch (Exception ex)
            {
                LogHelper.Error(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, "The Enhanced Markdown link parser had a issue", ex);
                return null;
            }
        }

    }
}
