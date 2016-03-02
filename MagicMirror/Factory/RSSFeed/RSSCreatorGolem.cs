using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Syndication;

namespace MagicMirror.Factory.RSSFeed
{
    public class RSSCreatorGolem : IRSSItemCreator
    {
        public string Name { get; } = "Golem";

        public Uri Url { get; set; } = new Uri("http://rss.golem.de/rss.php?feed=RSS2.0", UriKind.Absolute);

        public async Task<ViewModel.NewsItem> CreateItem(ViewModel.NewsItem result, SyndicationItem item)
        {
            result.URIToSource = item.Id;
            
            if (!string.IsNullOrEmpty(result.ContentRaw))
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(result.ContentRaw);
                foreach (HtmlNode link in doc.DocumentNode.Descendants("img"))
                {
                    result.Image = link.Attributes["src"].Value; break;
                }
                
                result.Content = HtmlEntity.DeEntitize(doc.DocumentNode.InnerText);
                result.ContentRaw = null;
            }

            HtmlDocument site = null;
            HtmlWeb web = new HtmlWeb();
            try { site = await web.LoadFromWebAsync(result.URIToSource); }
            catch { }

            if (site != null)
            {
                var contentNode = site.DocumentNode.Descendants("div").Where(A => A.Attributes["class"]?.Value?.Contains("formatted") ?? false).FirstOrDefault();

                HtmlNode imagecontainer;
                while ((imagecontainer = site.DocumentNode.Descendants("a").Where(A => A.Attributes["class"]?.Value?.Contains("golem-gallery2-nojs") ?? false).FirstOrDefault()) != null)
                {

                    string imageHtml = "<div>\n";
                    var imagesFromContainer = imagecontainer.Descendants("img");
                    foreach (var img in imagesFromContainer)
                    {
                        img.Attributes["src"].Value = img.Attributes["data-src-full"]?.Value ?? img.Attributes["data-src"]?.Value;
                        img.Attributes.Append("style", "max-width:600px");
                        imageHtml += img.OuterHtml + "<br/>\n";
                    }
                    imageHtml += "</div>\n";
                    var imagesNode = HtmlNode.CreateNode(imageHtml);
                    contentNode.ReplaceChild(imagesNode, imagecontainer);
                }


                if (contentNode != null && !string.IsNullOrEmpty(contentNode.InnerText) && !string.IsNullOrWhiteSpace(HtmlEntity.DeEntitize(contentNode.InnerText)))
                {
                    var nodes = contentNode.Elements("div").ToList();
                    foreach (var delNode in nodes)
                    {
                        contentNode.ChildNodes.Remove(delNode);
                    }

                    result.ContentRaw = string.Format(GenericHtml.HTML, contentNode.InnerHtml);
                }

                Func<string, string> getMetaContentByName = (string name) => site.DocumentNode.Descendants("meta")
                    ?.Where(A => A.Attributes["name"]?.Value?.ToLower() == name && !string.IsNullOrEmpty(A.Attributes["content"]?.Value))
                    ?.FirstOrDefault()?.Attributes["content"]?.Value;

                var twitter_image_src = getMetaContentByName("twitter:image:src");

                if (!string.IsNullOrEmpty(twitter_image_src))
                    result.Image = twitter_image_src;
            }

            return result;
        }
    }
}
